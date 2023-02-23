using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Data;
using System.Xml.Linq;

namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly IControlFlowInstance _controlFlowInstance;
        private readonly ICslQueryProvider _queryProvider;
        private readonly ICslAdminProvider _commandProvider;

        public ControlFlowRuntime(
            IControlFlowInstance controlFlowInstance,
            ICslQueryProvider queryProvider,
            ICslAdminProvider commandProvider)
        {
            _controlFlowInstance = controlFlowInstance;
            _queryProvider = queryProvider;
            _commandProvider = commandProvider;
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

        private async Task RunCapturableAsync(
            CaptureDeclaration declaration,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (declaration.Runnable.Query != null)
            {
                var reader = await _queryProvider.ExecuteQueryAsync(
                    string.Empty,
                    declaration.Runnable.Query,
                    new ClientRequestProperties());

                await CaptureResultAsync(declaration, reader, levelContext, ct);
            }
            else if (declaration.Runnable.Command != null)
            {
                var reader = await _commandProvider.ExecuteControlCommandAsync(
                    string.Empty,
                    declaration.Runnable.Query,
                    new ClientRequestProperties());

                await CaptureResultAsync(declaration, reader, levelContext, ct);
            }
            else
            {
                throw new NotSupportedException("runnable must be either query or command");
            }
        }

        private async Task CaptureResultAsync(
            CaptureDeclaration declaration,
            IDataReader reader,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var table = reader.ToDataSet().Tables[0];

            await levelContext.CompleteStepAsync(
                declaration.CaptureName,
                declaration.IsScalarCapture!.Value,
                table,
                ct);
        }
    }
}