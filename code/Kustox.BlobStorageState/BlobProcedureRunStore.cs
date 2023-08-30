using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kusto.Cloud.Platform.Utils.DecayCache;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRunStore : IProcedureRunStore
    {
        private readonly JsonLogBlob<RunData> _logBlob;

        public BlobProcedureRunStore(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient)
        {
            _logBlob = new JsonLogBlob<RunData>(
                rootFolder,
                containerClient,
                "procedure-run.json");
        }

        async Task IProcedureRunStore.CreateIfNotExistsAsync(CancellationToken ct)
        {
            await _logBlob.CreateIfNotExistsAsync(ct);
        }

        async Task<IImmutableList<ProcedureRun>> IProcedureRunStore.GetAllRunsAsync(
            CancellationToken ct)
        {
            var data = await _logBlob.ReadAllAsync(ct);
            var runs = data
                .Select(d => d.ToImmutable())
                .ToImmutableArray();

            return runs;
        }

        async Task<ProcedureRun?> IProcedureRunStore.GetLatestRunAsync(
            string jobId,
            CancellationToken ct)
        {
            var data = await _logBlob.ReadAllAsync(ct);
            var run = data
                .Where(r => r.JobId == jobId)
                .ArgMaxBy(r => r.Timestamp)
                ?.ToImmutable();

            return run;
        }

        async Task IProcedureRunStore.AppendRunAsync(
            IEnumerable<ProcedureRun> runs,
            CancellationToken ct)
        {
            var data = runs
                .Select(r => new RunData(r));

            await _logBlob.AppendAsync(data, ct);
        }
    }
}