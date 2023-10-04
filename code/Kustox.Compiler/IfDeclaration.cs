namespace Kustox.Compiler
{
    public class IfDeclaration: DeclarationCodeBase
    {
        public string Condition { get; set; } = string.Empty;

        public SequenceDeclaration ThenSequence { get; set; } = new SequenceDeclaration();
        
        public SequenceDeclaration? ElseSequence { get; set; }
    }
}