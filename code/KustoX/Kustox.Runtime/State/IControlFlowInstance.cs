using Kustox.Compiler;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State
{
    public interface IControlFlowInstance
    {
        long JobId { get; }

        Task CreateInstanceAsync(ControlFlowDeclaration declaration, CancellationToken ct);
        
        Task DeleteAsync(CancellationToken ct);

        Task<ControlFlowDeclaration> GetDeclarationAsync(CancellationToken ct);
        
        Task<TimestampedData<ControlFlowState>> GetControlFlowStateAsync(CancellationToken ct);
        
        Task<IImmutableList<ControlFlowStep>> GetStepsAsync(CancellationToken ct);
        
        Task SetControlFlowStateAsync(ControlFlowState state, CancellationToken ct);

        Task CompleteStepAsync(
            IImmutableList<int> indexes,
            string captureName,
            bool isScalarCapture,
            DataTable captureTable,
            CancellationToken ct);
    }
}