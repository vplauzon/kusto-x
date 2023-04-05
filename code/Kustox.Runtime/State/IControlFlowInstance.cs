using Kustox.Compiler;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State
{
    public interface IControlFlowInstance
    {
        long JobId { get; }

        Task CreateInstanceAsync(string script, CancellationToken ct);
        
        Task DeleteAsync(CancellationToken ct);

        Task<ControlFlowDeclaration> GetDeclarationAsync(CancellationToken ct);
        
        Task<TimestampedData<ProcedureRunState>> GetControlFlowStateAsync(CancellationToken ct);
        
        Task<IImmutableList<ControlFlowStep>> GetStepsAsync(
            IImmutableList<long> levelPrefix,
            CancellationToken ct);
        
        Task SetControlFlowStateAsync(ProcedureRunState state, CancellationToken ct);

        Task<ControlFlowStep> SetStepAsync(
            IImmutableList<long> indexes,
            StepState state,
            string script,
            string? captureName,
            TableResult? result,
            CancellationToken ct);
    }
}