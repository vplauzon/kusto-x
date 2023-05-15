using Kusto.Language;
using Kusto.Language.Syntax;

namespace Kustox.Compiler
{
    public abstract class RunnableDeclaration : RunnableDeclarationBase
    {
        internal override void Validate()
        {
            base.Validate();

            var capturableCount = (Command == null ? 0 : 1)
                + (Query == null ? 0 : 1);

            if (capturableCount != 1)
            {
                throw new InvalidDataException(
                    "Statement must be either a query or a command in "
                    + $" {typeof(RunnableDeclaration).Name}");
            }
        }
    }
}