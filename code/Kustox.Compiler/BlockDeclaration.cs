using Kusto.Language;
using Kusto.Language.Syntax;

namespace Kustox.Compiler
{
    public class BlockDeclaration : CommandOrQueryDeclarationBase
    {
        public CaptureDeclaration? Capture { get; set; }

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
            ForEach?.Validate();
        }

        internal override void SubParsing(KustoxCompiler compiler)
        {
            base.SubParsing(compiler);

            Capture?.SubParsing(compiler);
            ForEach?.SubParsing(compiler);
        }
    }
}