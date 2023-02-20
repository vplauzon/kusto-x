using Kustox.Compiler;

namespace Kustox.Runtime
{
    public interface IControlFlowInstance
    {
        Task SetupAsync(ControlFlowDeclaration declaration);
        
        Task DeleteAsync();

        Task<ControlFlowDeclaration> GetDeclarationAsync();
    }
}