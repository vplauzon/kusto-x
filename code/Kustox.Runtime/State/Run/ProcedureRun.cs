namespace Kustox.Runtime.State.Run
{
    public class ProcedureRun
    {
        public ProcedureRun(
            string jobId,
            ProcedureRunState state)
        {
            JobId = jobId;
            State = state;
        }

        public string JobId { get; }
        
        public ProcedureRunState State { get; }
    }
}