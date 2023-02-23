using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal class RuntimeContext
    {
        private readonly IControlFlowInstance _controlFlowInstance;
        private readonly IImmutableList<int> _stepPrefixes;
        private readonly IImmutableList<ControlFlowStep> _steps;

        #region Constructors
        private RuntimeContext(
            IControlFlowInstance controlFlowInstance,
            ControlFlowDeclaration declaration,
            IImmutableList<int> stepPrefixes,
            IImmutableList<ControlFlowStep> steps)
        {
            _controlFlowInstance = controlFlowInstance;
            Declaration = declaration;
            _stepPrefixes = stepPrefixes;
            _steps = steps;
        }

        public async static Task<RuntimeContext> LoadContextAsync(
            IControlFlowInstance controlFlowInstance,
            CancellationToken ct)
        {
            var declaration = await controlFlowInstance.GetDeclarationAsync(ct);
            var steps = await controlFlowInstance.GetStepsAsync(ct);
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

            return new RuntimeContext(
                controlFlowInstance,
                declaration,
                ImmutableArray<int>.Empty,
                steps);
        }
        #endregion

        public ControlFlowDeclaration Declaration { get; }

        public async Task EnsureStepsAsync(int stepCount, CancellationToken ct)
        {
            if(!_steps.Any())
            {
                //await  _controlFlowInstance.CreateStepsAsync(ct);
            }
        }
    }
}