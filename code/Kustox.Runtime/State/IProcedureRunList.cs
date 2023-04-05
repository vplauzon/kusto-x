using Kustox.Compiler;

namespace Kustox.Runtime.State
{
    public interface IProcedureRunList
    {

        IProcedureRun GetInstance(long jobId);
    }
}