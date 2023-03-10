namespace Kustox.Compiler
{
    public class PropertyDeclaration
    {
        public string Id { get; set; } = string.Empty;

        public bool? Boolean { get; set; }
        
        public int? Integer { get; set; }

        public string? String { get; set; }
    }
}