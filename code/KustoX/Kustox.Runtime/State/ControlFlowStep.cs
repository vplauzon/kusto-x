using System.Collections.Immutable;

namespace Kustox.Runtime.State
{
    public class ControlFlowStep
    {
        public ControlFlowStep(IImmutableList<long> stepBreadcrumb, StepState state, int retry)
        {
            StepBreadcrumb = stepBreadcrumb;
            State = state;
            Retry = retry;
        }

        public IImmutableList<long> StepBreadcrumb { get; }

        public StepState State { get; }

        public int Retry { get; }
    }
}