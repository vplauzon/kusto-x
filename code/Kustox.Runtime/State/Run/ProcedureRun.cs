namespace Kustox.Runtime.State.Run
{
    public class ProcedureRun
    {
        public ProcedureRun(
            string jobId)
        {
            JobId = jobId;
        }

        public string JobId { get; }
    }
}