using System.Collections.Immutable;

namespace Kustox.Compiler.Commands
{
    public class ShowProcedureRunsStepsResultCommandDeclaration : QueryableCommandBase
    {
        public string JobId { get; set; } = string.Empty;

        public IImmutableList<int> Steps { get; set; } = ImmutableArray<int>.Empty;

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(JobId))
            {
                throw new InvalidDataException("JobId unspecified");
            }
            if (Steps == null)
            {
                throw new InvalidDataException("Steps unspecified");
            }
            if (!Steps.Any())
            {
                throw new InvalidDataException("Empty steps");
            }
        }
    }
}