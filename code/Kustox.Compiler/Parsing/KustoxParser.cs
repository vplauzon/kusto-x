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
        private readonly Grammar _grammar = LoadGrammar();

        public StatementDeclaration? ParseStatement(string text)
        {
            return Parse<StatementDeclaration>("main", text);
        }

        public SequenceDeclaration? ParseProcedure(string text)
        {
            return Parse<SequenceDeclaration>("sequenceContent", text);
        }

        private T? Parse<T>(string? ruleName, string text)
        where T : class
        {
            var match = _grammar.Match(ruleName, text);

            if (match != null)
            {
                if (match.Text.Length != text.Length)
                {
                    throw new InvalidDataException($"Can't parse statement:  '{text}'");
                }
                else
                {
                    var plan = match.ComputeTypedOutput<T>();

                    return plan;
                }
            }
            else
            {
                return null;
            }
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

        private static Grammar LoadGrammar()
        {
            var text = LoadFileContent("Parsing.Kustox-grammar.txt");
            var grammar = MetaGrammar.ParseGrammar(text);

            if (grammar == null)
            {
                throw new NotSupportedException("Grammar couldn't be parsed");
            }

            return grammar;
        }
    }
}