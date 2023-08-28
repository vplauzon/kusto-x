namespace Kustox.Runtime.State.Run
{
    public interface IProcedureRunRegistry
    {
        IProcedureRun GetRun(long jobId);
    }
}