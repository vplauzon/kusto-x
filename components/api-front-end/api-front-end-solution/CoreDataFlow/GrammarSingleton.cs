using Microsoft.Extensions.FileProviders;
using PasLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CoreDataFlow
{
    public static class GrammarSingleton
    {
        public static Grammar Instance { get; } = LoadGrammar();

        private static Grammar LoadGrammar()
        {
            var assembly = typeof(GrammarSingleton).GetTypeInfo().Assembly;
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            using (var stream = embeddedProvider.GetFileInfo("dataflow-grammar.txt").CreateReadStream())
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                var grammar = MetaGrammar.ParseGrammar(text);

                return grammar;
            }
        }
    }
}