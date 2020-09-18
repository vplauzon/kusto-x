using PasLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreDataFlow
{
    public class Planner
    {
        private static readonly Grammar GRAMMAR = GetGrammar();

        public async Task ProcessRequestAsync(string text)
        {
            await ParseRequestAsync(text);
        }

        private Task ParseRequestAsync(string text)
        {
            GRAMMAR.Match("main", text);

            throw new NotImplementedException();
        }

        private static Grammar GetGrammar()
        {
            var assembly = typeof(Planner).GetTypeInfo().Assembly;
            var fullResourceName = "CorePlanning.dataflow-grammar.txt";

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