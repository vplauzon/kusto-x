namespace Kustox.Compiler.Commands
{
    public class ShowProcedureRunsCommandDeclaration : DeclarationBase
    {
        public QueryDeclaration? Query { get; set; }
        
        public string? JobId { get; set; }
    }
}