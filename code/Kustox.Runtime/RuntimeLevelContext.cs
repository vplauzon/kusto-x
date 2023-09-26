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
        private readonly IImmutableList<int> _stepBreadcrumb;
        private readonly IDictionary<int, RuntimeLevelContext> _existingSubContextMap;

        #region Constructors
        private RuntimeLevelContext(
            SharedData sharedData,
            IImmutableList<int> stepBreadcrumb,
            string script,
            IEnumerable<RuntimeLevelContext> subContexts,
            ProcedureRunStep? latestProcedureRunStep)
        {
            _sharedData = sharedData;
            _stepBreadcrumb = stepBreadcrumb;
            _existingSubContextMap = subContexts
                .ToDictionary(s => s._stepBreadcrumb.Last(), s => s);
            Script = script;
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
            //  Process in background
            var runTask = procedureRunStore.GetLatestRunsAsync(jobId, null, ct);
            var allSteps = await procedureRunStepStore.GetAllLatestStepsAsync(ct);
            ProcedureRunStep root;
            IImmutableDictionary<IImmutableList<int>, IImmutableList<ProcedureRunStep>> childrenMap;

            MapAllSteps(allSteps, jobId, out root, out childrenMap);

            var runs = await runTask;

            if (!runs.Any())
            {
                throw new InvalidDataException($"Job {jobId} doesn't exist");
            }

            var run = runs.First();

            await HandleRunStateAsync(procedureRunStore, jobId, run.State, ct);

            return CreateExistingContext(
                jobId,
                new SharedData(
                    procedureRunStepStore,
                    new StepCounter(maximumNumberOfSteps)),
                root,
                childrenMap);
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

        private static void MapAllSteps(
            IEnumerable<ProcedureRunStep> allSteps,
            string jobId,
            out ProcedureRunStep root,
            out IImmutableDictionary<IImmutableList<int>, IImmutableList<ProcedureRunStep>> childrenMap)
        {
            var comparer = new BreadcrumbComparer();

            if (!allSteps.Any())
            {
                throw new InvalidDataException($"No steps defined for job {jobId}");
            }
            var roots = allSteps
                .Where(s => !s.StepBreadcrumb.Any())
                .ToImmutableArray();

            childrenMap = allSteps
                .Where(s => s.StepBreadcrumb.Any())
                .GroupBy(s => s.StepBreadcrumb.SkipLast(1).ToImmutableArray(), comparer)
                .ToImmutableDictionary(g => g.Key, g => (IImmutableList<ProcedureRunStep>)g.ToImmutableArray(), comparer);

            if (!roots.Any())
            {
                throw new InvalidDataException(
                    $"Steps are corrupted for job {jobId}:  no root step");
            }
            if (roots.Count() > 1)
            {
                throw new InvalidDataException(
                    $"Steps are corrupted for job {jobId}:  many root steps");
            }

            root = roots.First();
        }

        private static RuntimeLevelContext CreateExistingContext(
            string jobId,
            SharedData sharedData,
            ProcedureRunStep root,
            IImmutableDictionary<IImmutableList<int>, IImmutableList<ProcedureRunStep>> childrenMap)
        {
            if (childrenMap.TryGetValue(root.StepBreadcrumb, out var childrenSteps))
            {
                var childrenContext = childrenSteps
                    .Select(s => CreateExistingContext(jobId, sharedData, s, childrenMap))
                    .ToImmutableArray();

                return new RuntimeLevelContext(
                    sharedData,
                    root.StepBreadcrumb,
                    root.Script,
                    childrenContext,
                    root);
            }
            else
            {
                return new RuntimeLevelContext(
                    sharedData,
                    root.StepBreadcrumb,
                    root.Script,
                    ImmutableArray<RuntimeLevelContext>.Empty,
                    root);
            }
        }
        #endregion
        #endregion

        public string Script { get; }

        public ProcedureRunStep? LatestProcedureRunStep { get; private set; }

        #region Level Management
        public RuntimeLevelContext GoDownOneLevel(int stepIndex, string script)
        {
            if (_existingSubContextMap.TryGetValue(stepIndex, out var subContext))
            {
                _existingSubContextMap.Remove(stepIndex);

                return subContext;
            }
            else
            {
                var subBreadcrumb = _stepBreadcrumb.Add(stepIndex);

                return new RuntimeLevelContext(
                    _sharedData,
                    subBreadcrumb,
                    script,
                    ImmutableArray<RuntimeLevelContext>.Empty,
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
            LatestProcedureRunStep = new ProcedureRunStep(
                Script,
                _stepBreadcrumb,
                StepState.Running,
                null,
                null,
                DateTime.UtcNow);

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
    }
}