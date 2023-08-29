using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRunStepStore : IProcedureRunStepStore
    {
        private readonly JsonLogBlob<StepData> _logBlob;
        private readonly string _jobId;

        public BlobProcedureRunStepStore(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient,
            string jobId)
        {
            _logBlob = new JsonLogBlob<StepData>(
                rootFolder,
                containerClient,
                "log.json");
            _jobId = jobId;
        }

        public async Task CreateAsync(CancellationToken ct)
        {
            await _logBlob.CreateIfNotExistsAsync(ct);
        }

        string IProcedureRunStepStore.JobId => _jobId;

        async Task<IImmutableList<ProcedureRunStep>> IProcedureRunStepStore.GetAllStepsAsync(
            CancellationToken ct)
        {
            var data = await _logBlob.ReadAllAsync(ct);
            var steps = data
                .Select(d => d.ToControlFlowStep())
                .ToImmutableArray();

            return steps;
        }

        async Task<IImmutableList<ProcedureRunStep>> IProcedureRunStepStore.GetAllLatestStepsAsync(
            CancellationToken ct)
        {
            var data = await _logBlob.ReadAllAsync(ct);
            var steps = data
                .GroupBy(r => string.Join('.', r.Breadcrumb))
                .Select(g => g.ArgMaxBy(r => r.Timestamp)!)
                .Select(d => d.ToControlFlowStep())
                .ToImmutableArray();

            return steps;
        }

        async Task IProcedureRunStepStore.AppendStepAsync(
            IEnumerable<ProcedureRunStep> steps,
            CancellationToken ct)
        {
            var data = steps
                .Select(s => new StepData(s));

            await _logBlob.AppendAsync(data, ct);
        }
    }
}