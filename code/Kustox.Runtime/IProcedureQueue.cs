using Kustox.Compiler;
using Kustox.Runtime.State.RunStep;

namespace Kustox.Runtime
{
    public interface IProcedureQueue
    {
        Task<IProcedureRunStepStore> QueueProcedureAsync(
            SequenceDeclaration procedureDeclaration,
            bool doNotRun,
            CancellationToken ct);
    }
}