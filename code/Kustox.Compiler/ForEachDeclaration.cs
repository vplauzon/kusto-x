using System.Collections.Immutable;

namespace Kustox.Compiler
{
    public class ForEachDeclaration: DeclarationCodeBase
    {
        //public IImmutableDictionary<string, object> Properties { get; set; } =
        //    ImmutableDictionary<string, object>.Empty;

        public string Cursor { get; set; } = string.Empty;
        
        public string Enumerator { get; set; } = string.Empty;

        public SequenceDeclaration Sequence { get; set; } = new SequenceDeclaration();
    }
}