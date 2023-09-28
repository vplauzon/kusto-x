using Kusto.Cloud.Platform.Data;
using Kusto.Language.Syntax;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;

namespace Kustox.Runtime.Commands
{
    internal class ShowProcedureRunStepsResultCommandRunner : CommandRunnerBase
    {
        private readonly IProcedureRunStepRegistry _procedureRunStepRegistry;

        public ShowProcedureRunStepsResultCommandRunner(
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
            var resultDeclaration = command.ShowProcedureRunsStepsResult!;
            var stepStore = _procedureRunStepRegistry.GetRun(resultDeclaration.JobId);
            var result = await stepStore.GetStepResultAsync(resultDeclaration.Steps, ct);

            if (resultDeclaration.Query == null)
            {
                return result;
            }
            else
            {
                var kqlResult = result?.ToKustoExpression();
                var tabularResult = result != null
                    ? (result.IsScalar ? $"print Scalar = {kqlResult}" : kqlResult)
                    : "print ['No Result']=''";
                var query = $@"
{tabularResult}
{resultDeclaration.GetPipedQuery()}";
                var reader = await ConnectionProvider.QueryProvider.ExecuteQueryAsync(
                    string.Empty,
                    query,
                    ConnectionProvider.EmptyClientRequestProperties);
                var table = reader.ToDataSet().Tables[0];

                return table.ToTableResult();
            }
        }
    }
}