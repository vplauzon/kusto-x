using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal class RuntimeLevelContext
    {
        #region Inner Types
        private record A();
        #endregion

        private readonly IControlFlowInstance _controlFlowInstance;
        private readonly IImmutableList<int> _levelPrefixes;
        private readonly List<ControlFlowStep> _currentLevelSteps;
        //private readonly ConcurrentDictionary<string, IImmutableList<ControlFlowStep>>
        //    _loadedAheadIndexedSteps;

        #region Constructors
        private RuntimeLevelContext(
            IControlFlowInstance controlFlowInstance,
            ControlFlowDeclaration declaration,
            IImmutableList<int> levelPrefixes,
            IImmutableList<ControlFlowStep> currentLevelSteps)
        {
            _controlFlowInstance = controlFlowInstance;
            Declaration = declaration;
            _levelPrefixes = levelPrefixes;
            _currentLevelSteps = currentLevelSteps.ToList();
        }

        public async static Task<RuntimeLevelContext> LoadContextAsync(
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

            return new RuntimeLevelContext(
                controlFlowInstance,
                declaration,
                ImmutableArray<int>.Empty,
                steps);
        }
        #endregion

        public ControlFlowDeclaration Declaration { get; }

        public IImmutableList<ControlFlowStep> GetSteps()
        {
            return _currentLevelSteps.ToImmutableList();
        }
    }
}