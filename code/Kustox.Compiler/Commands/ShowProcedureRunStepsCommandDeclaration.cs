namespace Kustox.Compiler.Commands
{
    public class ShowProcedureRunStepsCommandDeclaration : QueryableCommandBase
    {
        public string JobId { get; set; } = string.Empty;
    }
}