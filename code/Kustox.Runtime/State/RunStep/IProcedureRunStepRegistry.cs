namespace Kustox.Runtime.State.RunStep
{
    public interface IProcedureRunStepRegistry
    {
        Task<IProcedureRunStepStore> NewRunAsync(CancellationToken ct);
        
        IProcedureRunStepStore GetRun(string jobId);
    }
}