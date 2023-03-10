using Kusto.Language;

namespace Kustox.Compiler
{
    public class BlockDeclaration : DeclarationCodeBase
    {
        public CaptureDeclaration? Capture { get; set; }

        public ForEachDeclaration? ForEach { get; set; }

        public string? Query { get; set; }

        public string? Command { get; set; }

        public override void Validate()
        {
            var capturableCount = (ForEach == null ? 0 : 1)
                + (Query == null ? 0 : 1)
                + (Command == null ? 0 : 1);
            base.Validate();

            if (capturableCount != 1)
            {
                throw new InvalidDataException(
                    "Must have one and only one capturable in"
                    + $" {typeof(BlockDeclaration).Name}");
            }
            if (Query != null)
            {
                var code = KustoCode.Parse(Query);

                if (code.Kind != "Query")
                {
                    throw new InvalidDataException(
                        "Defined query isn't recognized as a query in "
                        + $" {typeof(BlockDeclaration).Name}:  '{Query}'");
                }
            }
            else if (Command != null)
            {
                var code = KustoCode.Parse(Command);

                if (code.Kind != "Command")
                {
                    throw new InvalidDataException(
                        "Defined command isn't recognized as a command in "
                        + $" {typeof(BlockDeclaration).Name}:  '{Command}'");
                }
            }

            Capture?.Validate();
        }
    }
}