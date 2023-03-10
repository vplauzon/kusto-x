using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State
{
    public class ControlFlowStep
    {
        public ControlFlowStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            long timestamp)
        {
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        public ControlFlowStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            string? captureName,
            TableResult? result,
            long timestamp) : this(stepBreadcrumb, state, timestamp)
        {
            CaptureName = captureName;
            Result = result;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public StepState State { get; }

        public string? CaptureName { get; }

        public TableResult? Result { get; }

        public DateTime Timestamp { get; }
    }
}