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
            "| project JobId, State, Timestamp";

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
            var tableScript = jobId == null
                ? "Run"
                : $@"
Run
| where JobId=='{jobId}'";
            var script = $@"
{tableScript}
| summarize arg_max(Timestamp,*) by JobId
| order by Timestamp asc
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
            var tableScript = jobId == null
                ? "Run"
                : $@"
Run
| where JobId=='{jobId}'";
            var script = $@"
{tableScript}
| order by Timestamp asc
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