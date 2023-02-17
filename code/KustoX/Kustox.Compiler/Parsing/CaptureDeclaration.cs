namespace Kustox.Compiler.Parsing
{
    internal class CaptureDeclaration
    {
        public string Id { get; set; } = string.Empty;

        public QueryOrCommandDeclaration QueryOrCommand { get; set; }
            = new QueryOrCommandDeclaration();
    }
}