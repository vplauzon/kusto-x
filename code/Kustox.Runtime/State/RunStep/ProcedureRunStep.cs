using System.Collections.Immutable;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace Kustox.Runtime.State.RunStep
{
    public class ProcedureRunStep
    {
        public ProcedureRunStep(
            string script,
            IImmutableList<int> stepBreadcrumb,
            StepState state,
            string? captureName,
            TableResult? result,
            DateTime timestamp)
        {
            Script = script;
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Timestamp = timestamp;
            CaptureName = captureName;
            Result = result;
        }

        public IImmutableList<int> StepBreadcrumb { get; }

        public string Script { get; }

        public StepState State { get; }

        public string? CaptureName { get; }

        public TableResult? Result { get; }

        public DateTime Timestamp { get; }
    }
}