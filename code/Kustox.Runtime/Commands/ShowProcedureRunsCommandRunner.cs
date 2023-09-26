using Kusto.Cloud.Platform.Data;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text;

namespace Kustox.Runtime.Commands
{
    internal class ShowProcedureRunsCommandRunner : CommandRunnerBase
    {
        private readonly IProcedureRunStore _procedureRunStore;

        public ShowProcedureRunsCommandRunner(
            ConnectionProvider connectionProvider,
            IProcedureRunStore procedureRunStore)
            : base(connectionProvider)
        {
            _procedureRunStore = procedureRunStore;
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            var result = await _procedureRunStore.QueryLatestRunsAsync(
                command.ShowProcedureRuns!.JobId,
                command.ShowProcedureRuns!.GetPipedQuery(),
                ct);

            return result;
        }
    }
}