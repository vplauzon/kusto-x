using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;

namespace Kustox.BlobStorageState.DataObjects
{
    internal class RunData
    {
        public RunData()
        {
        }

        public RunData(
            string jobId,
            ProcedureRunState state,
            DateTime timestamp)
        {
            JobId = jobId;
            State = state;
            Timestamp = timestamp;
        }

        public string JobId { get; set; } = string.Empty;

        public ProcedureRunState State { get; set; }

        public DateTime Timestamp { get; set; }

        public ProcedureRun ToImmutable()
        {
            return new ProcedureRun(JobId, State);
        }
    }
}