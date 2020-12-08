using PasLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoreControlFlow
{
    public class Planner
    {
        public async Task ProcessRequestAsync(string text)
        {
            await ParseRequestAsync(text);
        }

        private Task ParseRequestAsync(string text)
        {
            GrammarSingleton.Instance.Match("main", text);

            throw new NotImplementedException();
        }
    }
}