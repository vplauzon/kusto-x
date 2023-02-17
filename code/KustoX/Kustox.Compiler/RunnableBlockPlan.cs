namespace Kustox.Compiler
{
    public class RunnableBlockPlan : PlanBase
    {
        public string? CaptureName { get; set; }

        public bool? IsScalarCapture { get; set; }

        public string? Query { get; set; }

        public string? Command { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (IsScalarCapture != null && string.IsNullOrEmpty(CaptureName))
            {
                throw new InvalidDataException(
                    $"Inconsistant capture in {typeof(RunnableBlockPlan).Name}");
            }
            if (Query == null && Command == null)
            {
                throw new InvalidDataException(
                    "Must have at least one runnale in"
                    + $" {typeof(RunnableBlockPlan).Name}");
            }
        }
    }
}