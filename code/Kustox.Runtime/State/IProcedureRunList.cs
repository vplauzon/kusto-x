using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IProcedureRunList
    {

        IControlFlowInstance GetInstance(long jobId);
    }
}