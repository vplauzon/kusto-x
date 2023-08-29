using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    public class RuntimeLevelContext
    {
        #region Inner Types
        private class StepCounter
        {
            private readonly int? _maximumNumberOfSteps;
            private volatile int _stepCount = 0;

            public StepCounter(int? maximumNumberOfSteps)
            {
                _maximumNumberOfSteps = maximumNumberOfSteps;
            }

            public bool IncreaseStep()
            {
                if (_maximumNumberOfSteps != null)
                {
                    if (Interlocked.Increment(ref _stepCount) > _maximumNumberOfSteps)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private class SharedData
        {
            public SharedData(
                IProcedureRunStepStore procedureRunStepStore,
                ProcedureDeclaration declaration)
            {
                ProcedureRunStepStore = procedureRunStepStore;
                Declaration = declaration;
            }

            public IProcedureRunStepStore ProcedureRunStepStore { get; }

            public ProcedureDeclaration Declaration { get; }
        }
        #endregion

        private readonly SharedData _sharedData;
        private readonly IImmutableList<long> _levelPrefixes;
        private readonly IList<StepState> _stepStates;
        private readonly IDictionary<string, TableResult> _captures;
        private readonly StepCounter _stepCounter;

        #region Constructors
        private RuntimeLevelContext(
            SharedData sharedData,
            IImmutableList<long> levelPrefixes,
            IImmutableList<StepState> stepStates,
            IImmutableList<KeyValuePair<string, TableResult>> captures,
            StepCounter stepCounter)
        {
            _sharedData = sharedData;
            _levelPrefixes = levelPrefixes;
            _stepStates = stepStates.ToList();
            _captures = new Dictionary<string, TableResult>(captures);
            _stepCounter = stepCounter;
        }

        public async static Task<RuntimeLevelContext> LoadContextAsync(
            IProcedureRunStore procedureRunStore,
            IProcedureRunStepStore procedureRunStepStore,
            int? maximumNumberOfSteps,
            CancellationToken ct)
        {
            var jobId = procedureRunStepStore.JobId;
            var allStepsTask = procedureRunStepStore.GetAllLatestStepsAsync(ct);
            var run = await procedureRunStore.GetLatestRunAsync(jobId, ct);
            //  Sort steps in breadcrumb order
            var sortedSteps = (await allStepsTask)
                .OrderBy(s => s.StepBreadcrumb.Count)
                .ThenBy(s => s.StepBreadcrumb.Count == 0 ? -1 : s.StepBreadcrumb.Last())
                .ToImmutableArray();

            if (sortedSteps.Count() == 0)
            {
                throw new InvalidDataException($"No steps defined for job {jobId}");
            }
            if (sortedSteps.First().StepBreadcrumb.Count != 0)
            {
                throw new InvalidDataException(
                    $"Steps are corrupted for job {jobId}:  no root step");
            }

            var script = sortedSteps.First().Script;

            switch (run.State)
            {
                case ProcedureRunState.Pending:
                case ProcedureRunState.Paused:
                case ProcedureRunState.Error:
                    await procedureRunStore.AppendRunAsync(
                        new[]
                        {
                            new ProcedureRun(
                                jobId,
                                ProcedureRunState.Running)
                        },
                        ct);
                    break;
                case ProcedureRunState.Running:
                    break;
                case ProcedureRunState.Completed:
                    throw new InvalidOperationException(
                        $"Control flow (job id = '{jobId}') "
                        + "already is completed");
            }

            return new RuntimeLevelContext(
                new SharedData(
                    procedureRunStepStore,
                    declaration),
                ImmutableArray<long>.Empty,
                stepStates,
                captures,
                new StepCounter(maximumNumberOfSteps));
        }
        #endregion

        public ProcedureDeclaration Declaration => _sharedData.Declaration;

        #region Level Management
        public async Task<RuntimeLevelContext> GoDownOneLevelAsync(
            int stepIndex,
            CancellationToken ct)
        {
            var subPrefixes = _levelPrefixes.Add(stepIndex);
            var steps = await _controlFlowInstance.GetStepsAsync(subPrefixes, ct);
            var stepStates = steps
                .Select(s => s.State)
                .ToImmutableArray();

            return new RuntimeLevelContext(
                _controlFlowInstance,
                Declaration,
                subPrefixes,
                stepStates,
                _captures.ToImmutableArray(),
                _stepCounter);
        }
        #endregion

        public void PreStepExecution()
        {
            if (!_stepCounter.IncreaseStep())
            {
                throw new TaskCanceledException("Step Counter done");
            }
        }

        #region Step states
        public IImmutableList<StepState> GetLevelStepStates()
        {
            return _stepStates.ToImmutableArray();
        }

        public async Task<IImmutableList<ProcedureRunStep>> GetAllStepsAsync(CancellationToken ct)
        {
            var steps = await _controlFlowInstance.GetStepsAsync(_levelPrefixes, ct);

            return steps;
        }

        public async Task PersistRunningStepAsync(
            int stepIndex,
            string script,
            CancellationToken ct)
        {
            await _controlFlowInstance.SetStepAsync(
                _levelPrefixes.Add(stepIndex),
                StepState.Running,
                script,
                null,
                null,
                ct);

            if (stepIndex < _stepStates.Count())
            {
                _stepStates[stepIndex] = StepState.Running;
            }
            else
            {
                _stepStates.Add(StepState.Running);
            }
        }

        public async Task PersistCompleteStepAsync(
            int stepIndex,
            string script,
            string? captureName,
            TableResult result,
            CancellationToken ct)
        {
            await _controlFlowInstance.SetStepAsync(
                _levelPrefixes.Add(stepIndex),
                StepState.Completed,
                script,
                captureName,
                result,
                //  Do not cancel persistency here
                CancellationToken.None);

            if (captureName != null)
            {
                _captures.Add(captureName, result);
            }
            if (stepIndex < _stepStates.Count())
            {
                _stepStates[stepIndex] = StepState.Completed;
            }
            else
            {
                _stepStates.Add(StepState.Completed);
            }
            //  Compensate not taking the cancellation token into account in persistency
            if (ct.IsCancellationRequested)
            {
                throw new TaskCanceledException("After state persisted");
            }
        }
        #endregion

        #region Captured values
        public TableResult? GetCapturedValueIfExist(string name)
        {
            if (_captures.TryGetValue(name, out var result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public void AddCapturedValue(string cursor, TableResult item)
        {
            _captures.Add(cursor, item);
        }
        #endregion
    }
}