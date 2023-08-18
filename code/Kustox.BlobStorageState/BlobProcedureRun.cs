using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRun : IProcedureRun
    {
        private readonly JsonLogBlob<StepData> _logBlob;
        private readonly long _jobId;

        public BlobProcedureRun(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient,
            long jobId)
        {
            _logBlob = new JsonLogBlob<StepData>(rootFolder, containerClient, "log.json");
            _jobId = jobId;
        }

        long IProcedureRun.JobId => _jobId;

        async Task IProcedureRun.CreateRunAsync(string script, CancellationToken ct)
        {
            var data = new StepData(
                _jobId,
                ImmutableArray<long>.Empty,
                StepState.Running,
                script,
                null,
                null);

            await _logBlob.AppendAsync(ImmutableArray.Create(data), ct);
        }

        Task<TimestampedData<ProcedureRunState>> IProcedureRun.GetControlFlowStateAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task<ProcedureDeclaration> IProcedureRun.GetDeclarationAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task<IImmutableList<ProcedureRunStep>> IProcedureRun.GetStepsAsync(IImmutableList<long> levelPrefix, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task IProcedureRun.SetControlFlowStateAsync(ProcedureRunState state, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task<ProcedureRunStep> IProcedureRun.SetStepAsync(IImmutableList<long> indexes, StepState state, string script, string? captureName, TableResult? result, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}