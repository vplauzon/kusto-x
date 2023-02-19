using Kustox.Compiler;

namespace Kustox.Runtime
{
    public interface IControlFlowPersistency
    {
        Task<ControlFlowDeclaration> GetControlFlowDeclarationAsync(long jobId);
    }
}