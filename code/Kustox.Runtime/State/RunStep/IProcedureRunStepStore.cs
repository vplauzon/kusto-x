using Kustox.Compiler;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State.RunStep
{
    public interface IProcedureRunStepStore
    {
        string JobId { get; }

        Task<IImmutableList<ProcedureRunStep>> GetAllLatestStepsAsync(CancellationToken ct);

        Task AppendStepAsync(IEnumerable<ProcedureRunStep> steps, CancellationToken ct);
    }
}