namespace Kustox.Runtime.State.Run
{
    public class ProcedureRun
    {
        public ProcedureRun(
            string jobId,
            ProcedureRunState state,
            DateTime timestamp)
        {
            JobId = jobId;
            State = state;
            Timestamp = timestamp;
        }

        public string JobId { get; }

        public ProcedureRunState State { get; }

        public DateTime Timestamp { get; }
    }
}