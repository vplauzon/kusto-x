using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State.RunStep
{
    public class ProcedureRunStep
    {
        public ProcedureRunStep(
            string script,
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            DateTime timestamp)
        {
            Script = script;
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Timestamp = timestamp;
        }

        public ProcedureRunStep(
            string script,
            IImmutableList<long> stepBreadcrumb,
            StepState state,
            string? captureName,
            TableResult? result,
            DateTime timestamp) : this(script, stepBreadcrumb, state, timestamp)
        {
            CaptureName = captureName;
            Result = result;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public string Script { get; }

        public StepState State { get; }

        public string? CaptureName { get; }

        public TableResult? Result { get; }

        public DateTime Timestamp { get; }
    }
}