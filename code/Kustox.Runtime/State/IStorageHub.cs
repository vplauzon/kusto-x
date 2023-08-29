using Kustox.Compiler;
using Kustox.Runtime.State.RunStep;

namespace Kustox.Runtime.State
{
    public interface IStorageHub
    {
        IProcedureRunList ProcedureRunList { get; }

        IProcedureRunStepRegistry ProcedureRunRegistry { get; }
    }
}