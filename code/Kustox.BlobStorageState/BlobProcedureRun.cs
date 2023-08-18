using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRun : IProcedureRun
    {
        private long _jobId;

        public BlobProcedureRun(DataLakeDirectoryClient rootFolder, long jobId)
        {
            _jobId = jobId;
        }

        long IProcedureRun.JobId => _jobId;

        Task IProcedureRun.CreateRunAsync(string script, CancellationToken ct)
        {
            var data = new StepData(
                _jobId,
                ImmutableArray<long>.Empty,
                StepState.Running,
                script,
                null,
                null);

            throw new NotImplementedException();
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