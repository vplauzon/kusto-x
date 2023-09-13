using Kustox.Compiler;
using Kustox.Runtime.State.RunStep;

namespace Kustox.Runtime.Commands
{
    internal class RunProcedureCommandRunner : CommandRunnerBase
    {
        public RunProcedureCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            //command.RunProcedureCommand.RootSequence;
            throw new NotImplementedException();
        }
    }
}