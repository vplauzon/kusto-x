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
        private readonly List<ControlFlowStep> _levelSteps;
        private readonly StepCounter _stepCounter;

        #region Constructors
        private RuntimeLevelContext(
            IControlFlowInstance controlFlowInstance,
            ControlFlowDeclaration declaration,
            IImmutableList<long> levelPrefixes,
            IImmutableList<ControlFlowStep> levelSteps,
            StepCounter stepCounter)
        {
            _controlFlowInstance = controlFlowInstance;
            Declaration = declaration;
            _levelPrefixes = levelPrefixes;
            _levelSteps = levelSteps.ToList();
            _stepCounter = stepCounter;
        }

        public async static Task<RuntimeLevelContext> LoadContextAsync(
            IControlFlowInstance controlFlowInstance,
            int? maximumNumberOfSteps,
            CancellationToken ct)
        {
            var declaration = await controlFlowInstance.GetDeclarationAsync(ct);
            var state = await controlFlowInstance.GetControlFlowStateAsync(ct);
            var steps = await controlFlowInstance.GetStepsAsync(ImmutableArray<long>.Empty, ct);

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
                steps,
                new StepCounter(maximumNumberOfSteps));
        }
        #endregion

        public ControlFlowDeclaration Declaration { get; }

        public IImmutableList<ControlFlowStep> GetSteps()
        {
            return _levelSteps.ToImmutableList();
        }

        public async Task CompleteStepAsync(
            string captureName,
            bool isScalarCapture,
            DataTable table,
            CancellationToken ct)
        {
            if (_levelSteps.Count != 1)
            {
                throw new InvalidOperationException(
                    "This should only be called in the context of a single step");
            }
            await _controlFlowInstance.SetStepAsync(
                _levelPrefixes,
                StepState.Completed,
                _levelSteps.First().Retry,
                captureName,
                isScalarCapture,
                table,
                ct);

            if (!_stepCounter.IncreaseStep())
            {
                throw new TaskCanceledException("Step Counter done");
            }
        }

        public async Task<RuntimeLevelContext> GoToOneStepAsync(
            int index,
            CancellationToken ct)
        {
            var lowerStep = index >= _levelSteps.Count()
                ? null
                : _levelSteps[index];
            var newPrefixes = _levelPrefixes.Add(index);
            //  Persist the running state
            var newStep = await _controlFlowInstance.SetStepAsync(
                newPrefixes,
                StepState.Running,
                lowerStep == null ? 0 : lowerStep.Retry + 1,
                null,
                null,
                null,
                ct);

            return new RuntimeLevelContext(
                _controlFlowInstance,
                Declaration,
                _levelPrefixes.Add(index),
                new[] { newStep }.ToImmutableArray(),
                _stepCounter);
        }
    }
}