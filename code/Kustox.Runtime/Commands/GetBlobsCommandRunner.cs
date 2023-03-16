using Kustox.Compiler;
using Kustox.Runtime.State;

namespace Kustox.Runtime.Commands
{
    internal class GetBlobsCommandRunner : CommandRunnerBase
    {
        public GetBlobsCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override Task<TableResult> RunCommandAsync(
            BlockDeclaration block,
            bool isScalarCapture,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}