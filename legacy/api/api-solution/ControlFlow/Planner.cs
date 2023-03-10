using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ControlFlow
{
    public class Planner
    {
        public async Task ProcessRequestAsync(string text)
        {
            await ParseRequestAsync(text);
        }

        private Task ParseRequestAsync(string text)
        {
            var declaration = LanguageParser.ParseExtendedCommand(text);

            throw new NotImplementedException();
        }
    }
}