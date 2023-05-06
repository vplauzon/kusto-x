using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    public class KustoRuntime
    {
        public async Task<TableResult> RunStatementAsync(
            string statement,
            CancellationToken ct)
        {
            await Task.CompletedTask;

            throw new NotImplementedException();
        }
    }
}