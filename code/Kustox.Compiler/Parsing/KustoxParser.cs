using GramParserLib;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Compiler.Parsing
{
    internal class KustoxParser
    {
        private readonly Grammar _controlFlowGrammar = LoadControlFlowGrammar();
        private readonly Grammar _commandTypeGrammar = LoadCommandTypeGrammar();
        private readonly Grammar _commandGrammar = LoadCommandGrammar();

        public ProcedureDeclaration? ParseControlFlow(string text)
        {
            var match = _controlFlowGrammar.Match("main", text);

            if (match != null)
            {
                if (match.Text.Length != text.Length)
                {
                    throw new InvalidDataException($"Can't parse control flow:  '{text}'");
                }
                else
                {
                    var plan = match.ComputeTypedOutput<ProcedureDeclaration>();

                    return plan;
                }
            }
            else
            {
                return null;
            }
        }

        public ExtendedCommandType ParseCommandType(string command)
        {
            var match = _commandTypeGrammar.Match("main", command);

            if (match != null)
            {
                var output = match.ComputeTypedOutput<string>();

                if (output != null)
                {
                    if (Enum.TryParse<ExtendedCommandType>(output, out var commandType))
                    {
                        return commandType;
                    }
                }
            }

            return ExtendedCommandType.Kusto;
        }

        public CommandDeclaration ParseExtendedCommands(string command)
        {
            var match = _commandGrammar.Match("main", command);

            if (match != null)
            {
                var output = match.ComputeTypedOutput<CommandDeclaration>();

                if (output != null)
                {
                    return output;
                }
            }

            return new CommandDeclaration();
        }

        private static string LoadFileContent(string path)
        {
            var assembly = typeof(KustoxParser).GetTypeInfo().Assembly;
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using (var stream = embeddedProvider
                .GetFileInfo(path)
                .CreateReadStream())
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Can't find grammar file");
                }
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd();

                    return text;
                }
            }
        }

        private static Grammar LoadControlFlowGrammar()
        {
            var tokenText = LoadFileContent("Parsing.Token-grammar.txt");
            var controlFlowText = LoadFileContent("Parsing.Controlflow-grammar.txt");
            var grammar = MetaGrammar.ParseGrammar(tokenText + controlFlowText);

            if (grammar == null)
            {
                throw new NotSupportedException("Control flow grammar couldn't be parsed");
            }

            return grammar;
        }

        private static Grammar LoadCommandTypeGrammar()
        {
            var tokenText = LoadFileContent("Parsing.Token-grammar.txt");
            var commandTypeText = LoadFileContent("Parsing.CommandType-grammar.txt");
            var grammar = MetaGrammar.ParseGrammar(tokenText + commandTypeText);

            if (grammar == null)
            {
                throw new NotSupportedException("Command type grammar couldn't be parsed");
            }

            return grammar;
        }

        private static Grammar LoadCommandGrammar()
        {
            var tokenText = LoadFileContent("Parsing.Token-grammar.txt");
            var commandText = LoadFileContent("Parsing.Command-grammar.txt");
            var grammar = MetaGrammar.ParseGrammar(tokenText + commandText);

            if (grammar == null)
            {
                throw new NotSupportedException("Command grammar couldn't be parsed");
            }

            return grammar;
        }
    }
}