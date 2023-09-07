using Kusto.Language;
using Kusto.Language.Syntax;

namespace Kustox.Compiler
{
    public class StatementDeclaration : DeclarationCodeBase
    {
        public QueryDeclaration? Query { get; set; }

        public CommandDeclaration? Command { get; set; }

        internal override void Validate()
        {
            base.Validate();

            var capturableCount = (Command == null ? 0 : 1)
                + (Query == null ? 0 : 1);

            if (capturableCount != 1)
            {
                throw new InvalidDataException(
                    "Statement must be either a query or a command in "
                    + $" {typeof(StatementDeclaration).Name}");
            }
            Query?.Validate();
            Command?.Validate();
        }
    }
}