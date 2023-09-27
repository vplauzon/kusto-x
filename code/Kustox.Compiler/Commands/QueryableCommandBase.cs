using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Compiler.Commands
{
    public abstract class QueryableCommandBase : DeclarationBase
    {
        public QueryDeclaration? Query { get; set; }

        public string? GetPipedQuery()
        {
            if (Query != null)
            {
                return $"| {Query.Code}";
            }
            else
            {
                return null;
            }
        }
    }
}
