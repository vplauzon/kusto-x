namespace Kustox.Compiler.Parsing
{
    internal class CaptureDeclaration
    {
        public string Id { get; set; } = string.Empty;

        public RunnableDeclaration Runnable { get; set; }
            = new RunnableDeclaration();
    }
}