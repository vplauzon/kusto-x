using Kusto.Cloud.Platform.Data;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text;

namespace Kustox.Runtime.Commands
{
    internal class ShowProcedureRunStepsCommandRunner : CommandRunnerBase
    {
        private readonly IProcedureRunStepRegistry _procedureRunStepRegistry;

        public ShowProcedureRunStepsCommandRunner(
            ConnectionProvider connectionProvider,
            IStorageHub storageHub)
            : base(connectionProvider)
        {
            _procedureRunStepRegistry = storageHub.ProcedureRunRegistry;
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            var stepsDeclaration = command.ShowProcedureRunsSteps!;
            var stepStore = _procedureRunStepRegistry.GetRun(stepsDeclaration.JobId);
            var result = await stepStore.QueryLatestRunsAsync(
                stepsDeclaration.GetPipedQuery(),
                ct);

            return result;
        }
    }
}