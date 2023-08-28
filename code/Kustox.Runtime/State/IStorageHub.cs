using Kustox.Compiler;
using Kustox.Runtime.State.Run;

namespace Kustox.Runtime.State
{
    public interface IStorageHub
    {
        IProcedureRunList ProcedureRunList { get; }

        IProcedureRunRegistry ProcedureRunRegistry { get; }
    }
}