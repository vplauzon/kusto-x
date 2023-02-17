using System.Collections.Immutable;

namespace Kustox.Compiler.Parsing
{
    internal class ControlFlowDeclaration
    {
        public IImmutableList<GroupingItemDeclaration> GroupingContent { get; set; }
            = new ImmutableArray<GroupingItemDeclaration>();
    }
}