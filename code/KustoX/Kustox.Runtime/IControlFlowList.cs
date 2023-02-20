using Kustox.Compiler;

namespace Kustox.Runtime
{
    public interface IControlFlowList
    {
        IControlFlowInstance GetInstance(long jobId);
    }
}