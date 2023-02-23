using System.Collections.Immutable;

namespace Kustox.Runtime.State
{
    public class ControlFlowStep
    {
        public ControlFlowStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            int retry,
            long timestamp)
        {
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Retry = retry;
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public StepState State { get; }

        public int Retry { get; }
     
        public DateTime Timestamp { get; }
    }
}