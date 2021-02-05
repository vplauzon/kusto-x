using GramParserLib;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ControlFlow
{
    public static class LanguageParser
    {
        private static readonly Grammar _grammarInstance = LoadGrammar();

        public static ControlFlowDeclaration? ParseDeclaration(string text)
        {
            var match = _grammarInstance.Match("main", text);

            if (match != null)
            {
                var declaration = match.ComputeTypedOutput<ControlFlowDeclaration>();

                return declaration;
            }
            else
            {
                return null;
            }
        }

        private static Grammar LoadGrammar()
        {
            var assembly = typeof(LanguageParser).GetTypeInfo().Assembly;
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using (var stream = embeddedProvider.GetFileInfo("dataflow-grammar.txt").CreateReadStream())
            {
                if (stream == null)
                {
                    throw new FileNotFoundException("Can't find grammar file");
                }
                using (var reader = new StreamReader(stream))
                {
                    var text = reader.ReadToEnd();
                    var grammar = MetaGrammar.ParseGrammar(text);

                    if (grammar == null)
                    {
                        throw new NotSupportedException("Grammar couldn't be parsed");
                    }

                    return grammar;
                }
            }
        }
    }
}