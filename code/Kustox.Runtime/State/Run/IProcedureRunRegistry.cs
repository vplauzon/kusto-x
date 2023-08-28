namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunRegistry
    {
        Task<IProcedureRun> NewRunAsync(CancellationToken ct);
        
        Task<IProcedureRun> GetRunAsync(string jobId, CancellationToken ct);
    }
}