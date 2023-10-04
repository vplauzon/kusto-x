using Kusto.Cloud.Platform.Data;
using Kusto.Language.Syntax;
using Kustox.KustoState.DataObjects;
using Kustox.Runtime;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Kustox.KustoState
{
    internal class KustoProcedureRunStore : IProcedureRunStore
    {
        private const string TABLE_NAME = "Run";
        private const string PROJECT_CLAUSE =
            "| project JobId, State, StartedOn, Timestamp, Duration";

        private readonly ConnectionProvider _connectionProvider;
        private readonly StreamingBuffer _streamingBuffer;

        public KustoProcedureRunStore(ConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            _streamingBuffer = new StreamingBuffer(
                _connectionProvider.StreamingIngestClient,
                _connectionProvider.QueryProvider.DefaultDatabaseName,
                TABLE_NAME);
        }

        async Task<ProcedureRun?> IProcedureRunStore.GetRunAsync(
            string jobId,
            CancellationToken ct)
        {
            var script = $@"
Run
| where JobId=='{jobId}'
| summarize arg_max(Timestamp,*) by JobId
";
            var runsData = await KustoHelper.QueryAsync<RunData>(
                _connectionProvider.QueryProvider,
                script,
                ct);
            var runs = runsData
                .Select(r => r.ToImmutable());
            var run = runs.FirstOrDefault();

            return run;
        }

        async Task<TableResult> IProcedureRunStore.QueryRunsAsync(
            string? jobId,
            string? query,
            CancellationToken ct)
        {
            var jobClause = jobId != null ? $"| where JobId=='{jobId}'" : null;
            var script = $@"
Run
| summarize arg_max(Timestamp,*) by JobId
| join kind=inner (Run
    | summarize arg_min(Timestamp,*) by JobId
    | project-rename StartedOn=Timestamp) on JobId
| extend Duration=iif(State=='Completed', Timestamp-StartedOn, timespan(null))
{jobClause}
| order by Timestamp desc
{PROJECT_CLAUSE}
{query}";
            var runsData = await _connectionProvider.QueryProvider.ExecuteQueryAsync(
                string.Empty,
                script,
                _connectionProvider.EmptyClientRequestProperties,
                ct);
            var table = runsData.ToDataSet().Tables[0].ToTableResult();

            return table;
        }

        async Task<TableResult> IProcedureRunStore.QueryRunHistoryAsync(
            string jobId,
            string? query,
            CancellationToken ct)
        {
            var script = $@"
Run
| where JobId == '{jobId}'
| order by Timestamp desc
{query}";
            var runsData = await _connectionProvider.QueryProvider.ExecuteQueryAsync(
                string.Empty,
                script,
                _connectionProvider.EmptyClientRequestProperties,
                ct);
            var table = runsData.ToDataSet().Tables[0].ToTableResult();

            return table;
        }

        async Task IProcedureRunStore.AppendRunAsync(
            IEnumerable<ProcedureRun> runs,
            CancellationToken ct)
        {
            var data = runs
                .Select(r => new RunData(r));

            await _streamingBuffer.AppendRecords(data, ct);
        }
    }
}