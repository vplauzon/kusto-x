namespace Kustox.Compiler
{
    public class BlockPlan : PlanBase
    {
        public RunnableBlockPlan? Runnable { get; }

        public override void Validate()
        {
            base.Validate();

            if (Runnable == null)
            {
                throw new InvalidDataException(
                    $"At least one element of {typeof(BlockPlan).Name} must be present");
            }
        }
    }
}