using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunStore
    {
        Task<ProcedureRun?> GetRunAsync(string jobId, CancellationToken ct);
        
        Task<TableResult> QueryRunsAsync(
            string? jobId,
            string? query,
            CancellationToken ct);

        Task<TableResult> QueryRunHistoryAsync(
            string jobId,
            string? query,
            CancellationToken ct);

        Task AppendRunAsync(IEnumerable<ProcedureRun> runs, CancellationToken ct);
    }
}