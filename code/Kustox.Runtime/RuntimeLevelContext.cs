﻿using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
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
                    if (Interlocked.Increment(ref _stepCount) > _maximumNumberOfSteps)
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
        private readonly IList<StepState> _stepStates;
        private readonly IDictionary<string, TableResult> _captures;
        private readonly StepCounter _stepCounter;

        #region Constructors
        private RuntimeLevelContext(
            IControlFlowInstance controlFlowInstance,
            ControlFlowDeclaration declaration,
            IImmutableList<long> levelPrefixes,
            IImmutableList<StepState> stepStates,
            IImmutableList<KeyValuePair<string, TableResult>> captures,
            StepCounter stepCounter)
        {
            _controlFlowInstance = controlFlowInstance;
            Declaration = declaration;
            _levelPrefixes = levelPrefixes;
            _stepStates = stepStates.ToList();
            _captures = new Dictionary<string, TableResult>(captures);
            _stepCounter = stepCounter;
        }

        public async static Task<RuntimeLevelContext> LoadContextAsync(
            IControlFlowInstance controlFlowInstance,
            int? maximumNumberOfSteps,
            CancellationToken ct)
        {   //  Run in parallel
            var declarationTask = controlFlowInstance.GetDeclarationAsync(ct);
            var stateTask = controlFlowInstance.GetControlFlowStateAsync(ct);
            var stepsTask = controlFlowInstance.GetStepsAsync(ImmutableArray<long>.Empty, ct);
            var declaration = await declarationTask;
            var state = await stateTask;
            var steps = await stepsTask;
            var stepStates = steps
                .Select(s => s.State)
                .ToImmutableArray();
            var captures = steps
                .Where(s => !string.IsNullOrWhiteSpace(s.CaptureName))
                .Select(s => KeyValuePair.Create(s.CaptureName, s.Result))
                .ToImmutableArray();

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
                stepStates,
                captures,
                new StepCounter(maximumNumberOfSteps));
        }
        #endregion

        public ControlFlowDeclaration Declaration { get; }

        public void PreStepExecution()
        {
            if (!_stepCounter.IncreaseStep())
            {
                throw new TaskCanceledException("Step Counter done");
            }
        }

        public async Task RunningStepAsync(int stepIndex, string script, CancellationToken ct)
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

        public async Task CompleteStepAsync(
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

        public IImmutableList<StepState> GetLevelStepStates()
        {
            return _stepStates.ToImmutableArray();
        }
    }
}