using Kusto.Language;
using Kusto.Language.Syntax;

namespace Kustox.Compiler
{
    public class BlockDeclaration : DeclarationCodeBase
    {
        public CaptureDeclaration? Capture { get; set; }

        public ForEachDeclaration? ForEach { get; set; }

        public string? Query { get; set; }

        public string? Command { get; set; }

        public CommandType CommandType { get; set; }

        public GetBlobDeclaration? GetBlobs { get; set; }

        internal override void Validate()
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
                switch (CommandType)
                {
                    case CommandType.Kusto:
                        var code = KustoCode.Parse(Command);

                        if (code.Kind != "Command")
                        {
                            throw new InvalidDataException(
                                "Defined command isn't recognized as a command in "
                                + $" {typeof(BlockDeclaration).Name}:  '{Command}'");
                        }
                        break;
                    case CommandType.GetBlobs:
                        if (GetBlobs == null)
                        {
                            throw new InvalidDataException($"Syntax error:  '{Command}'");
                        }
                        GetBlobs.Validate();
                        break;
                    default:
                        throw new NotSupportedException($"Command type:  '{CommandType}'");
                }
            }

            Capture?.Validate();
        }

        internal override void SubParsing(KustoxCompiler compiler)
        {
            base.SubParsing(compiler);

            Capture?.SubParsing(compiler);
            ForEach?.SubParsing(compiler);

            if (Command != null)
            {
                var block = compiler.ParseCommand(Command);

                CommandType = block.CommandType;
                GetBlobs = block.GetBlobs;
            }
        }
    }
}