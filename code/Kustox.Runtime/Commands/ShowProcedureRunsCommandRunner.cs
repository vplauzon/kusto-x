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
            var runs = await _procedureRunStore.GetLatestRunsAsync(
                command.ShowProcedureRuns!.JobId,
                command.ShowProcedureRuns!.Query?.Code,
                ct);
            var columns = ImmutableArray<ColumnSpecification>
                .Empty
                .Add(new ColumnSpecification("JobId", typeof(string)))
                .Add(new ColumnSpecification("State", typeof(string)))
                .Add(new ColumnSpecification("Timestamp", typeof(DateTime)));
            var table = new TableResult(
                false,
                columns,
                runs
                .Select(r => ImmutableArray<object>.Empty.Add(r.JobId).Add(r.State.ToString()).Add(r.Timestamp))
                .Cast<IImmutableList<object>>()
                .ToImmutableArray());

            return table;
        }
    }
}