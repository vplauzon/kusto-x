using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IControlFlowInstance
    {
        Task CreateInstanceAsync(ControlFlowDeclaration declaration, CancellationToken ct);
        
        Task DeleteAsync(CancellationToken ct);

        Task<ControlFlowDeclaration> GetDeclarationAsync(CancellationToken ct);
    }
}