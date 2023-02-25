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
            DataTable? captureTable,
            long timestamp)
        {
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Retry = retry;
            CaptureName = captureName;
            IsScalarCapture = isScalarCapture;
            CaptureTable = captureTable;
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public StepState State { get; }

        public int Retry { get; }

        public string? CaptureName { get; set; }

        public bool? IsScalarCapture { get; set; }

        public DataTable? CaptureTable { get; set; }

        public DateTime Timestamp { get; }
    }
}