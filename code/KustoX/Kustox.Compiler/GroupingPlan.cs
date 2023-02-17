using System.Collections.Immutable;

namespace Kustox.Compiler
{
    public class GroupingPlan : PlanBase
    {
        public IImmutableList<BlockPlan> Blocks { get; set; }
            = new ImmutableArray<BlockPlan>();
    }
}