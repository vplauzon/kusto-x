using Kustox.Runtime.State.RunStep;

namespace Kustox.Runtime
{
    public interface IProcedureQueue
    {
        Task<IProcedureRunStepStore> QueueProcedureAsync(
            string script,
            bool doNotRun,
            CancellationToken ct);
    }
}