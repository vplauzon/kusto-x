namespace Kustox.Compiler.Commands
{
    public class ShowProcedureRunsStepsCommandDeclaration : QueryableCommandBase
    {
        public string JobId { get; set; } = string.Empty;

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(JobId))
            {
                throw new InvalidDataException("JobId unspecified");
            }
        }
    }
}