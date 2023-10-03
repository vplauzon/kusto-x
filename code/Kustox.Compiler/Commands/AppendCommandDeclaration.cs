using System.Collections.Immutable;

namespace Kustox.Compiler.Commands
{
    public class AppendCommandDeclaration : DeclarationBase
    {
        public string TableName { get; set; } = string.Empty;

        public string CaptureId { get; set; } = string.Empty;

        public IImmutableList<PropertyDeclaration> Properties { get; set; } =
            ImmutableArray<PropertyDeclaration>.Empty;

        internal override void Validate()
        {
            base.Validate();

            if (Properties == null)
            {
                throw new InvalidDataException($"No '{nameof(Properties)}'");
            }
            foreach (var property in Properties)
            {
                property.Validate();
            }
            if (string.IsNullOrWhiteSpace(TableName))
            {
                throw new InvalidDataException($"No '{nameof(TableName)}'");
            }
            if (string.IsNullOrWhiteSpace(CaptureId))
            {
                throw new InvalidDataException($"No '{nameof(CaptureId)}'");
            }
        }
    }
}