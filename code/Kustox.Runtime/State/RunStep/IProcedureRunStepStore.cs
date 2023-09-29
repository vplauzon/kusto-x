using Kustox.Compiler;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State.RunStep
{
    public interface IProcedureRunStepStore
    {
        string JobId { get; }

        Task<IImmutableList<ProcedureRunStep>> GetAllStepsAsync(CancellationToken ct);

        Task<TableResult> QueryStepsAsync(
            string? query,
            IImmutableList<int>? stepBreadcrumb,
            CancellationToken ct);
        
        Task<TableResult> QueryRunResultAsync(
            string? query,
            CancellationToken ct);
        
        Task<TableResult> QueryStepResultAsync(
            string? query,
            IImmutableList<int> stepBreadcrumb,
            CancellationToken ct);

        Task<TableResult> QueryStepHistoryAsync(
            string? query,
            IImmutableList<int> stepBreadcrumb,
            CancellationToken ct);

        Task AppendStepAsync(IEnumerable<ProcedureRunStep> steps, CancellationToken ct);
    }
}