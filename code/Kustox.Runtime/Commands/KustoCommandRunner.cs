using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kusto.Language.Syntax;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.Commands
{
    internal class KustoCommandRunner : CommandRunnerBase
    {
        public KustoCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override async Task<TableResult> RunCommandAsync(
            string commandScript,
            bool isScalarCapture,
            CancellationToken ct)
        {
            var reader = await ConnectionProvider.CommandProvider.ExecuteControlCommandAsync(
                string.Empty,
                commandScript,
                new ClientRequestProperties());
            var table = reader.ToDataSet().Tables[0];
            var result = new TableResult(isScalarCapture, table);

            return result;
        }
    }
}
