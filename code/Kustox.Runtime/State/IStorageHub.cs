using Kustox.Compiler;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;

namespace Kustox.Runtime.State
{
    public interface IStorageHub
    {
        IProcedureRunStore ProcedureRunStore { get; }

        IProcedureRunStepRegistry ProcedureRunRegistry { get; }
    }
}