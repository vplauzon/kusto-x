using Kusto.Language;

namespace Kustox.Compiler
{
    public class RunnableDeclaration : DeclarationBase
    {
        public RunnableDeclaration() : base(false)
        {
        }

        public string? Query { get; set; }

        public string? Command { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (Query == null && Command == null)
            {
                throw new InvalidDataException(
                    "Must have at least one runnale in"
                    + $" {typeof(RunnableDeclaration).Name}");
            }
            if (Query != null && Command != null)
            {
                throw new InvalidDataException(
                    "Can't have both query & command in one runnale in "
                    + $" {typeof(RunnableDeclaration).Name}");
            }
            if (Query != null)
            {
                var code = KustoCode.Parse(Query);

                if (code.Kind != "Query")
                {
                    throw new InvalidDataException(
                        "Defined query isn't recognized as a query in "
                        + $" {typeof(RunnableDeclaration).Name}:  '{Query}'");
                }
            }
            else
            {
                var code = KustoCode.Parse(Command);

                if (code.Kind != "Command")
                {
                    throw new InvalidDataException(
                        "Defined command isn't recognized as a command in "
                        + $" {typeof(RunnableDeclaration).Name}:  '{Command}'");
                }
            }
        }
    }
}