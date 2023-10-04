using Kusto.Language;

namespace Kustox.Compiler.Commands
{
    public class DeleteCommandDeclaration : DeclarationBase
    {
        public string TableName { get; set; } = string.Empty;

        public QueryDeclaration? Query { get; set; }

        internal override void Validate()
        {
            base.Validate();

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