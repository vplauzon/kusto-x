using System.Collections.Immutable;

namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunStore
    {
        Task<ProcedureRun?> GetLatestRunAsync(string jobId, CancellationToken ct);

        Task AppendRunAsync(IEnumerable<ProcedureRun> runs, CancellationToken ct);
    }
}