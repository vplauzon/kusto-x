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
            string? captureName,
            bool? isScalarCapture,
            DataTable? result,
            long timestamp)
        {
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Retry = retry;
            CaptureName = captureName;
            IsScalarCapture = isScalarCapture;
            Result = result;
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public StepState State { get; }

        public int Retry { get; }

        public string? CaptureName { get; }

        public bool? IsScalarCapture { get; }

        public DataTable? Result { get; }

        public DateTime Timestamp { get; }
    }
}