using System.Collections.Immutable;

namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunStore
    {
        Task CreateIfNotExistsAsync(CancellationToken ct);

        Task<IImmutableList<ProcedureRun>> GetAllRunsAsync(CancellationToken ct);

        Task<ProcedureRun?> GetLatestRunAsync(string jobId, CancellationToken ct);

        Task AppendRunAsync(IEnumerable<ProcedureRun> runs, CancellationToken ct);
    }
}