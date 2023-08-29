using System.Collections.Immutable;

namespace Kustox.Runtime.State.RunList
{
    public interface IProcedureRunStore
    {
        Task<IImmutableList<ProcedureRun>> GetSummariesAsync(CancellationToken ct);

        Task AppendStepAsync(IEnumerable<ProcedureRun> summaries, CancellationToken ct);
    }
}