using Kusto.Language;
using Kusto.Language.Syntax;

namespace Kustox.Compiler
{
    public class BlockDeclaration : DeclarationCodeBase
    {
        public CaptureDeclaration? Capture { get; set; }

        public QueryDeclaration? Query { get; set; }

        public CommandDeclaration? Command { get; set; }

        public ForEachDeclaration? ForEach { get; set; }

        internal override void Validate()
        {
            base.Validate();
            
            var capturableCount = (ForEach == null ? 0 : 1)
                + (Command == null ? 0 : 1)
                + (Query == null ? 0 : 1);

            if (capturableCount != 1)
            {
                throw new InvalidDataException(
                    "Must have one and only one capturable in"
                    + $" {typeof(BlockDeclaration).Name}");
            }
            Capture?.Validate();
            Query?.Validate();
            Command?.Validate();
            ForEach?.Validate();
        }
    }
}