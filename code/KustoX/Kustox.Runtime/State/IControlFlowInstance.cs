using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IControlFlowInstance
    {
        Task CreateInstanceAsync(ControlFlowDeclaration declaration);
        
        Task DeleteAsync();

        Task<ControlFlowDeclaration> GetDeclarationAsync();
    }
}