using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IControlFlowList
    {
        IControlFlowInstance GetInstance(long jobId);
    }
}