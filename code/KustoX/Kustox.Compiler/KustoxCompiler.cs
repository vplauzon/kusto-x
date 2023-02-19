using Kustox.Compiler.Parsing;

namespace Kustox.Compiler
{
    public class KustoxCompiler
    {
        private readonly KustoxParser _parser = new KustoxParser();

        public ControlFlowDeclaration? CompileScript(string controlFlowScript)
        {
            var declaration = _parser.ParseControlFlow(controlFlowScript);

            if(declaration != null)
            {
                declaration.Validate();
            }

            return declaration;
        }
    }
}