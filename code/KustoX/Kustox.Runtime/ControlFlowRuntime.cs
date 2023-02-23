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

        private async Task RunInstructionAsync(
            BlockDeclaration instuction,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (instuction.Capturable != null)
            {
                await RunCapturableAsync(instuction.Capturable, levelContext, ct);
            }
            else
            {
                throw new NotSupportedException("Block declaration");
            }
        }

        private async Task RunSequenceAsync(
            SequenceDeclaration sequence,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            //var steps = levelContext.GetSteps();
            var blocks = sequence.Blocks;

            for (int i = 0; i != blocks.Count(); ++i)
            {
                var instuction = blocks[i];

                await RunInstructionAsync(instuction, levelContext, ct);
            }
        }

        private Task RunCapturableAsync(
            CaptureDeclaration declaration,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (declaration.Runnable.Query != null)
            {
                throw new NotImplementedException();
            }
            else if (declaration.Runnable.Command != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException("runnable must be either query or command");
            }
        }
    }
}