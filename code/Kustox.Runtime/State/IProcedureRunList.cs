using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IProcedureRunList
    {
        IProcedureRun GetRun(long jobId);
    }
}