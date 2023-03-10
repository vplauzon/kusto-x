using System.Collections.Immutable;

namespace Kustox.Compiler
{
    public class ForEachDeclaration: DeclarationCodeBase
    {
        public IImmutableList<PropertyDeclaration> Properties { get; set; } =
            ImmutableArray<PropertyDeclaration>.Empty;

        public string Cursor { get; set; } = string.Empty;
        
        public string Enumerator { get; set; } = string.Empty;

        public SequenceDeclaration Sequence { get; set; } = new SequenceDeclaration();

        public int Concurrency { get; set; } = 1;
    }
}