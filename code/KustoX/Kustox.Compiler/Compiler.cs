using Kustox.Compiler.Parsing;

namespace Kustox.Compiler
{
    public class Compiler
    {
        private readonly LanguageParser _parser = new LanguageParser();

        public ControlFlowPlan Compile(string controlFlowScript)
        {
            var declaration = _parser.ParseControlFlow(controlFlowScript);
            //var plan = new 

            throw new NotImplementedException();
        }
    }
}