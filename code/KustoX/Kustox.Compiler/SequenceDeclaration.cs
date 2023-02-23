using System.Collections.Immutable;

namespace Kustox.Compiler
{
    public class SequenceDeclaration : DeclarationBase
    {
        public IImmutableList<BlockDeclaration> Blocks { get; set; }
            = new ImmutableArray<BlockDeclaration>();

        public override void Validate()
        {
            base.Validate();

            foreach (var block in Blocks)
            {
                block.Validate();
            }
        }
    }
}