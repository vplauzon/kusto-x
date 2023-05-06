using Kustox.Compiler.Parsing;

namespace Kustox.Compiler
{
    public class KustoxCompiler
    {
        private readonly KustoxParser _parser = new KustoxParser();

        public ProcedureDeclaration? CompileScript(string controlFlowScript)
        {
            var declaration = _parser.ParseControlFlow(controlFlowScript);

            if (declaration != null)
            {
                declaration.SubParsing(this);
                declaration.Validate();
            }

            return declaration;
        }

        public CommandDeclaration ParseCommand(string command)
        {
            var commandType = _parser.ParseCommandType(command);

            if (commandType == ExtendedCommandType.Kusto)
            {
                return new CommandDeclaration
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