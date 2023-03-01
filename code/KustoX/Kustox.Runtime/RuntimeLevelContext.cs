using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal class RuntimeLevelContext
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
                    if (Interlocked.Increment(ref _stepCount) >= _maximumNumberOfSteps)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        #endregion

        private readonly IControlFlowInstance _controlFlowInstance;
        private readonly IImmutableList<long> _levelPrefixes;
        private readonly IDictionary<string, TableResult> _captures;
        private readonly StepCounter _stepCounter;

        #region Constructors
        private RuntimeLevelContext(
            IControlFlowInstance controlFlowInstance,
            ControlFlowDeclaration declaration,
            IImmutableList<long> levelPrefixes,
            IImmutableList<KeyValuePair<string, TableResult>> captures,
            StepCounter stepCounter)
        {
            _controlFlowInstance = controlFlowInstance;
            Declaration = declaration;
            _levelPrefixes = levelPrefixes;
            _captures = new Dictionary<string, TableResult>(captures);
            _stepCounter = stepCounter;
        }

        public async static Task<RuntimeLevelContext> LoadContextAsync(
            IControlFlowInstance controlFlowInstance,
            int? maximumNumberOfSteps,
            CancellationToken ct)
        {
            var declaration = await controlFlowInstance.GetDeclarationAsync(ct);
            var state = await controlFlowInstance.GetControlFlowStateAsync(ct);

            switch (state.Data)
            {
                case ControlFlowState.Pending:
                case ControlFlowState.Paused:
                case ControlFlowState.Error:
                    await controlFlowInstance.SetControlFlowStateAsync(
                        ControlFlowState.Running,
                        ct);
                    break;
                case ControlFlowState.Running:
                    break;
                case ControlFlowState.Completed:
                    throw new InvalidOperationException(
                        $"Control flow (job id = '{controlFlowInstance.JobId}')");
            }

            return new RuntimeLevelContext(
                controlFlowInstance,
                declaration,
                ImmutableArray<long>.Empty,
                ImmutableArray<KeyValuePair<string, TableResult>>.Empty,
                new StepCounter(maximumNumberOfSteps));
        }
        #endregion

        public ControlFlowDeclaration Declaration { get; }

        public async Task<IImmutableList<ControlFlowStep>> RestoreStepsAsync(CancellationToken ct)
        {
            var steps = await _controlFlowInstance.GetStepsAsync(_levelPrefixes, ct);

            foreach (var step in steps)
            {
                if (step.CaptureName != null)
                {
                    _captures.Add(step.CaptureName!, step.Result!);
                }
            }

            return steps;
        }

        public async Task CompleteStepAsync(
            int stepIndex,
            string? captureName,
            TableResult result,
            CancellationToken ct)
        {
            await _controlFlowInstance.SetStepAsync(
                _levelPrefixes.Add(stepIndex),
                StepState.Completed,
                captureName,
                result.IsScalar,
                result.ToDataTable(),
                //  Do not cancel persistency here
                CancellationToken.None);

            if (captureName != null)
            {
                _captures.Add(captureName, result);
            }
            //  Compensate not taking the cancellation token into account in persistency
            if (ct.IsCancellationRequested)
            {
                throw new TaskCanceledException("After state persisted");
            }
            if (!_stepCounter.IncreaseStep())
            {
                throw new TaskCanceledException("Step Counter done");
            }
        }

        public IImmutableList<KeyValuePair<string, TableResult>> GetCapturedValues(
            ImmutableArray<string> nameReferences)
        {
            var list = new List<KeyValuePair<string, TableResult>>();

            foreach (var name in nameReferences)
            {
                TableResult? result;

                if (_captures.TryGetValue(name, out result))
                {
                    list.Add(KeyValuePair.Create(name, result));
                }
            }

            return list.ToImmutableArray();
        }
    }
}