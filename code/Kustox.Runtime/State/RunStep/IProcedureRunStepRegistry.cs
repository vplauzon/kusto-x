namespace Kustox.Runtime.State.RunStep
{
    public interface IProcedureRunStepRegistry
    {
        Task<IProcedureRunStepStore> NewRunAsync(CancellationToken ct);
        
        Task<IProcedureRunStepStore> GetRunAsync(string jobId, CancellationToken ct);
    }
}