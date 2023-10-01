using System.Collections.Immutable;

namespace Kustox.Compiler.Commands
{
    public class ShowProcedureRunsCommandDeclaration : QueryableCommandBase
    {
        public string? JobId { get; set; }

        public IImmutableList<int>? Breadcrumb { get; set; }

        public bool IsSteps { get; set; } = false;

        public bool IsResult { get; set; } = false;

        public bool IsHistory { get; set; } = false;

        public bool IsChildren { get; set; } = false;

        internal override void Validate()
        {
            base.Validate();

            if (IsResult && IsHistory)
            {
                throw new InvalidDataException("Can't have both result and history");
            }
            if (IsResult && IsChildren)
            {
                throw new InvalidDataException("Can't have both result and children");
            }
            if (IsChildren && IsHistory)
            {
                throw new InvalidDataException("Can't have both children and history");
            }
            if ((IsSteps || IsResult || IsHistory || IsChildren)
                && string.IsNullOrWhiteSpace(JobId))
            {
                throw new InvalidDataException("Job ID must be specified");
            }
            if (IsChildren && !IsSteps)
            {
                throw new InvalidDataException("Children available only in steps");
            }
            if ((IsChildren || IsHistory || IsResult) && IsSteps && Breadcrumb == null)
            {
                throw new InvalidDataException("Step sequence must be provided");
            }
        }
    }
}