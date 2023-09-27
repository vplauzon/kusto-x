namespace Kustox.Compiler.Commands
{
    public class ShowProcedureRunsStepsCommandDeclaration : QueryableCommandBase
    {
        public string JobId { get; set; } = string.Empty;
    }
}