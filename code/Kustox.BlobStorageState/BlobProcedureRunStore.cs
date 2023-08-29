using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRunStore : IProcedureRunStore
    {
        Task IProcedureRunStore.AppendRunAsync(
            IEnumerable<ProcedureRun> runs,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task<IImmutableList<ProcedureRun>> IProcedureRunStore.GetAllRunsAsync(
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        Task<ProcedureRun> IProcedureRunStore.GetLatestRunAsync(
            string jobId,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}