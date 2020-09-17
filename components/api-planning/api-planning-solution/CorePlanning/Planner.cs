using PasApiClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanning
{
    public class Planner
    {
        private static readonly string GRAMMAR = GetGrammar();

        public async Task ProcessRequestAsync(string text)
        {
            await ParseRequestAsync(text);
        }

        private Task ParseRequestAsync(string text)
        {
            var client = ParserClient.Create();

            client.SingleParseAsync(GRAMMAR, text);

            throw new NotImplementedException();
        }

        private static string GetGrammar()
        {
            var assembly = typeof(Planner).GetTypeInfo().Assembly;
            var fullResourceName = "CorePlanning.dataflow-grammar.txt";

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();

                return text;
            }
        }
    }
}