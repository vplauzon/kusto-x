namespace Kustox.Compiler.Parsing
{
    internal class GroupingItemDeclaration
    {
        public CaptureDeclaration? CaptureScalar { get; set; }
        
        public CaptureDeclaration? CaptureTable { get; set; }
    }
}