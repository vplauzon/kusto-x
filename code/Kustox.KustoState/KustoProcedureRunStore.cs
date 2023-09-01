using Kustox.KustoState.DataObjects;
using Kustox.Runtime;
using Kustox.Runtime.State.Run;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.KustoState
{
    internal class KustoProcedureRunStore : IProcedureRunStore
    {
        private const string TABLE_NAME = "Run";

        private readonly ConnectionProvider _connectionProvider;

        public KustoProcedureRunStore(ConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        async Task IProcedureRunStore.AppendRunAsync(
            IEnumerable<ProcedureRun> runs,
            CancellationToken ct)
        {
            var data = runs
                .Select(r => new RunData(r));

            await KustoHelper.StreamIngestAsync(
                _connectionProvider.StreamingIngestClient,
                _connectionProvider.QueryProvider.DefaultDatabaseName,
                TABLE_NAME,
                data,
                ct);
        }

        async Task<ProcedureRun?> IProcedureRunStore.GetLatestRunAsync(
            string jobId,
            CancellationToken ct)
        {
            var runsData = await KustoHelper.QueryAsync<RunData>(
                _connectionProvider.QueryProvider,
                $"Run | where JobId=='{jobId}' | summarize arg_max(Timestamp,*) by JobId",
                ct);
            var runs = runsData
                .Select(r => r.ToImmutable());
            var run = runs.FirstOrDefault();

            return run;
        }
    }
}