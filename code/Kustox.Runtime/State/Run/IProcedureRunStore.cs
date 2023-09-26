using System.Collections.Immutable;

namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunStore
    {
        Task<IImmutableList<ProcedureRun>> GetLatestRunsAsync(
            string? jobId,
            string? query,
            CancellationToken ct);

        Task AppendRunAsync(IEnumerable<ProcedureRun> runs, CancellationToken ct);
    }
}