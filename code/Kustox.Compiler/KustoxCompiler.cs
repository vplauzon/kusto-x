using Kustox.Compiler.Parsing;

namespace Kustox.Compiler
{
    public class KustoxCompiler
    {
        private readonly KustoxParser _parser = new KustoxParser();

        public ControlFlowDeclaration? CompileScript(string controlFlowScript)
        {
            var declaration = _parser.ParseControlFlow(controlFlowScript);

            if (declaration != null)
            {
                declaration.SubParsing(this);
                declaration.Validate();
            }

            return declaration;
        }

        public BlockDeclaration ParseCommand(string command)
        {
            var commandType = _parser.ParseCommandType(command);

            if (commandType == CommandType.Kusto)
            {
                return new BlockDeclaration
                {
                    CommandType = commandType
                };
            }
            else
            {
                var declaration = _parser.ParseExtendedCommands(command);

                declaration.CommandType = commandType;

                return declaration;
            }
        }
    }
}