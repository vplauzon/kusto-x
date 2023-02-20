using Kustox.Compiler;

namespace Kustox.Runtime
{
    public interface IControlFlowPersistency
    {
        Task<ControlFlowDeclaration> SetupAsync(ControlFlowDeclaration declaration);

        Task<ControlFlowDeclaration> GetDeclarationAsync();
    }
}