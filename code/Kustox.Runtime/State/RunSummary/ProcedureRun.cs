namespace Kustox.Runtime.State.RunList
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