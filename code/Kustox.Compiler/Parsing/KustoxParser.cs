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
        private readonly Grammar _grammarInstance = LoadGrammar();

        public ControlFlowDeclaration? ParseControlFlow(string text)
        {
            var match = _grammarInstance.Match("main", text);

            if (match != null)
            {
                var plan = match.ComputeTypedOutput<ControlFlowDeclaration>();

                return plan;
            }
            else
            {
                return null;
            }
        }

        private static Grammar LoadGrammar()
        {
            var assembly = typeof(KustoxParser).GetTypeInfo().Assembly;
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using (var stream = embeddedProvider
                .GetFileInfo("Parsing.Controlflow-grammar.txt")
                .CreateReadStream())
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