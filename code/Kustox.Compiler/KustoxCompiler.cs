using Kustox.Compiler.Parsing;

namespace Kustox.Compiler
{
    public class KustoxCompiler
    {
        private readonly static KustoxParser _parser = new KustoxParser();

        public StatementDeclaration? CompileStatement(string text)
        {
            var declaration = _parser.ParseStatement(text);

            if (declaration != null)
            {
                declaration.Validate();
            }

            return declaration;
        }

        public SequenceDeclaration? CompileProcedure(string text)
        {
            var declaration = _parser.ParseProcedure(text);

            if (declaration != null)
            {
                declaration.Validate();
            }

            return declaration;
        }
    }
}