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
            var assembly = typeof(Planner).GetTypeInfo().Assembly;
            var fullResourceName = typeof(GrammarSingleton).Name + ".dataflow-grammar.txt";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                var grammar = MetaGrammar.ParseGrammar(text);

                return grammar;
            }
        }
    }
}