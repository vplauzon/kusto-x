using System.Collections.Immutable;
using System.Data;

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

        public ControlFlowStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            int retry,
            string? captureName,
            TableResult? result,
            long timestamp) : this(stepBreadcrumb, state, retry, timestamp)
        {
            CaptureName = captureName;
            Result = result;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public StepState State { get; }

        public int Retry { get; }

        public string? CaptureName { get; }

        public TableResult? Result { get; }

        public DateTime Timestamp { get; }
    }
}