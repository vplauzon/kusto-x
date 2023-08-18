using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State
{
    public class ProcedureRunStep
    {

        public ProcedureRunStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            long timestamp) : this(
                stepBreadcrumb,
                state,
                DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime)
        {
        }

        public ProcedureRunStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            string? captureName,
            TableResult? result,
            long timestamp) : this(
                stepBreadcrumb,
                state,
                captureName,
                result,
                DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime)
        {
        }

        public ProcedureRunStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            DateTime timestamp)
        {
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Timestamp = timestamp;
        }

        public ProcedureRunStep(
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            string? captureName,
            TableResult? result,
            DateTime timestamp) : this(stepBreadcrumb, state, timestamp)
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