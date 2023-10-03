using System.Collections.Immutable;

namespace Kustox.Compiler.Commands
{
    public class AppendCommandDeclaration : DeclarationBase
    {
        public string TableName { get; set; } = string.Empty;

        public QueryDeclaration? Query { get; set; }

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
            if (Query == null)
            {
                throw new InvalidDataException($"No '{nameof(Query)}'");
            }
            Query.Validate();
        }
    }
}