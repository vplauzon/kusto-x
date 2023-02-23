using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Xml.Linq;

namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly IControlFlowInstance _controlFlowInstance;

        public ControlFlowRuntime(IControlFlowInstance controlFlowInstance)
        {
            _controlFlowInstance = controlFlowInstance;
        }

        public async Task RunAsync(CancellationToken ct = default(CancellationToken))
        {
            var levelContext =
                await RuntimeLevelContext.LoadContextAsync(_controlFlowInstance, ct);

            await RunSequenceAsync(
                levelContext.Declaration.RootSequence,
                levelContext,
                ct);
        }

        private async Task RunSequenceAsync(
            SequenceDeclaration sequence,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            await levelContext.EnsureStepsAsync(sequence.Blocks.Count(), ct);
         
            foreach (var instruction in sequence.Blocks)
            {
            }
        }
    }
}