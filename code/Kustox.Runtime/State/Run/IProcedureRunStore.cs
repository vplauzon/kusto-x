using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunStore
    {
        Task<ProcedureRun?> GetLatestRunAsync(string jobId, CancellationToken ct);
        
        Task<TableResult> QueryLatestRunsAsync(
            string? jobId,
            string? query,
            CancellationToken ct);

        Task AppendRunAsync(IEnumerable<ProcedureRun> runs, CancellationToken ct);
    }
}