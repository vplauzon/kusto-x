using Kustox.Compiler.Parsing;

namespace Kustox.Compiler
{
    public class Compiler
    {
        private readonly LanguageParser _parser = new LanguageParser();

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