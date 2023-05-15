using Kusto.Language;
using Kusto.Language.Syntax;

namespace Kustox.Compiler
{
    public abstract class RunnableDeclarationBase : DeclarationCodeBase
    {
        public QueryDeclaration? Query { get; set; }

        public CommandDeclaration? Command { get; set; }

        internal override void Validate()
        {
            base.Validate();

            Query?.Validate();
            Command?.Validate();
        }

        internal override void SubParsing(KustoxCompiler compiler)
        {
            base.SubParsing(compiler);

            Query?.SubParsing(compiler);
            Command?.SubParsing(compiler);
        }
    }
}