using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    /// <summary>
    /// A context is mapped to a step (i.e. step breadcrumb).
    /// When loaded, it will hold the existing steps.  Steps are exposed as a
    /// concurrent stack.  This is meant to save memory by quite quickly unstacking
    /// the ones that have succeeded.
    /// 
    /// Context also keep track of captures.
    /// 
    /// New sub steps can be created.
    /// </summary>
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
                StepCounter stepCounter)
            {
                ProcedureRunStepStore = procedureRunStepStore;
                StepCounter = stepCounter;
            }

            public IProcedureRunStepStore ProcedureRunStepStore { get; }

            public StepCounter StepCounter { get; }
        }
        #endregion

        private readonly SharedData _sharedData;
        private readonly IImmutableList<long> _stepBreadcrumb;
        private readonly ConcurrentStack<RuntimeLevelContext> _existingSubContextStack;

        #region Constructors
        private RuntimeLevelContext(
            SharedData sharedData,
            IImmutableList<long> stepBreadcrumb,
            string script,
            ConcurrentStack<RuntimeLevelContext> subContextStack,
            IImmutableDictionary<string, TableResult> captures,
            ProcedureRunStep? latestProcedureRunStep)
        {
            _sharedData = sharedData;
            _stepBreadcrumb = stepBreadcrumb;
            _existingSubContextStack = subContextStack;
            Script = script;
            Captures = captures;
            LatestProcedureRunStep = latestProcedureRunStep;
        }

        #region Load Context
        public async static Task<RuntimeLevelContext> LoadContextAsync(
            IProcedureRunStore procedureRunStore,
            IProcedureRunStepStore procedureRunStepStore,
            int? maximumNumberOfSteps,
            CancellationToken ct)
        {
            var jobId = procedureRunStepStore.JobId;
            var runTask = procedureRunStore.GetLatestRunAsync(jobId, ct);
            var sortedSteps = await LoadSortedStepsAsync(procedureRunStepStore, jobId, ct);
            var run = await runTask;

            if (run == null)
            {
                throw new InvalidDataException($"Job {jobId} doesn't exist");
            }
            await HandleRunStateAsync(procedureRunStore, jobId, run.State, ct);

            return CreateExistingContext(
                new SharedData(
                    procedureRunStepStore,
                    new StepCounter(maximumNumberOfSteps)),
                sortedSteps,
                0,
                sortedSteps.Count(),
                ImmutableDictionary<string, TableResult>.Empty);
        }

        private static async Task HandleRunStateAsync(
            IProcedureRunStore procedureRunStore,
            string jobId,
            ProcedureRunState currentState,
            CancellationToken ct)
        {
            switch (currentState)
            {
                case ProcedureRunState.Pending:
                case ProcedureRunState.Paused:
                case ProcedureRunState.Error:
                    await procedureRunStore.AppendRunAsync(
                        new[]
                        {
                            new ProcedureRun(
                                jobId,
                                ProcedureRunState.Running,
                                DateTime.UtcNow)
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
        }

        private static async Task<ImmutableArray<ProcedureRunStep>> LoadSortedStepsAsync(
            IProcedureRunStepStore procedureRunStepStore,
            string jobId,
            CancellationToken ct)
        {
            var allSteps = await procedureRunStepStore.GetAllLatestStepsAsync(ct);
            //  Sort steps in breadcrumb order
            var sortedSteps = allSteps
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

            return sortedSteps;
        }

        private static RuntimeLevelContext CreateExistingContext(
            SharedData sharedData,
            ImmutableArray<ProcedureRunStep> sortedSteps,
            int index,
            int length,
            IImmutableDictionary<string, TableResult> captures)
        {
            var root = sortedSteps[index];
            var rootDepth = root.StepBreadcrumb.Count;
            var subContexts = new List<RuntimeLevelContext>();
            var subIndex = index + 1;
            var subLength = 0;
            var subCaptures = captures;

            while (subIndex + subLength < length)
            {
                ++subLength;
                if (subIndex + subLength + 1 >= length
                    || sortedSteps[subIndex + subLength + 1].StepBreadcrumb.Count == rootDepth + 1)
                {
                    var subStep = sortedSteps[subIndex];

                    subContexts.Add(CreateExistingContext(
                        sharedData,
                        sortedSteps,
                        subIndex,
                        subLength,
                        subCaptures));
                    subIndex = subIndex + subLength;
                    subLength = 0;
                    if (subStep.CaptureName != null && subStep.Result != null)
                    {
                        subCaptures = subCaptures.Add(subStep.CaptureName, subStep.Result);
                    }
                }
            }

            return new RuntimeLevelContext(
                sharedData,
                root.StepBreadcrumb,
                root.Script,
                new ConcurrentStack<RuntimeLevelContext>(subContexts.AsEnumerable().Reverse()),
                root.CaptureName != null && root.Result != null
                ? captures.Add(root.CaptureName, root.Result)
                : captures,
                root);
        }
        #endregion
        #endregion

        public string Script { get; }

        public IImmutableDictionary<string, TableResult> Captures { get; }

        public ProcedureRunStep? LatestProcedureRunStep { get; private set; }

        #region Level Management
        public RuntimeLevelContext GoDownOneLevel(
            int stepIndex,
            string script,
            IImmutableDictionary<string, TableResult> captures)
        {
            if (_existingSubContextStack.TryPop(out var subContext))
            {
                if (!subContext._stepBreadcrumb.Any()
                    || subContext._stepBreadcrumb.Last() != stepIndex)
                {
                    throw new InvalidDataException("Corrupted data, invalid breadcrumb");
                }

                return subContext;
            }
            else
            {
                var subBreadcrumb = _stepBreadcrumb.Add(stepIndex);

                return new RuntimeLevelContext(
                    _sharedData,
                    subBreadcrumb,
                    script,
                    new ConcurrentStack<RuntimeLevelContext>(),
                    captures,
                    null);
            }
        }
        #endregion

        public void PreStepExecution()
        {
            if (!_sharedData.StepCounter.IncreaseStep())
            {
                throw new TaskCanceledException("Step Counter done");
            }
        }

        #region Step states
        public async Task PersistRunningStepAsync(CancellationToken ct)
        {
            LatestProcedureRunStep =
                new ProcedureRunStep(Script, _stepBreadcrumb, StepState.Running, DateTime.UtcNow);

            await _sharedData.ProcedureRunStepStore.AppendStepAsync(
                new[]
                {
                    LatestProcedureRunStep
                },
                ct);
        }

        public async Task PersistCompleteStepAsync(
            string? captureName,
            TableResult result,
            CancellationToken ct)
        {
            LatestProcedureRunStep = new ProcedureRunStep(
                Script,
                _stepBreadcrumb,
                StepState.Completed,
                captureName,
                result,
                DateTime.UtcNow);

            await _sharedData.ProcedureRunStepStore.AppendStepAsync(
                new[]
                {
                    LatestProcedureRunStep
                },
                //  Do not cancel persistency here
                CancellationToken.None);

            //  Compensate not taking the cancellation token into account in persistency
            if (ct.IsCancellationRequested)
            {
                throw new TaskCanceledException("After state persisted");
            }
        }
        #endregion

        public TableResult? GetCapturedValueIfExist(string name)
        {
            if (Captures.TryGetValue(name, out var result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
    }
}