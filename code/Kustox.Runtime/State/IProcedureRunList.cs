using Kustox.Compiler;
using Kustox.Runtime.State.Run;

namespace Kustox.Runtime.State
{
    public interface IProcedureRunList
    {
        IProcedureRun GetRun(long jobId);
    }
}