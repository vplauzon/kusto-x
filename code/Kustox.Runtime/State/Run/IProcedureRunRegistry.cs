namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunRegistry
    {
        IProcedureRun NewRun();
        
        IProcedureRun GetRun(string jobId);
    }
}