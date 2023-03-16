using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kusto.Language.Syntax;
using Kustox.Compiler;
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
        private static readonly ClientRequestProperties _emptyProperties =
            new ClientRequestProperties();

        public KustoCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override async Task<TableResult> RunCommandAsync(
            BlockDeclaration block,
            bool isScalarCapture,
            CancellationToken ct)
        {
            var reader = await ConnectionProvider.CommandProvider.ExecuteControlCommandAsync(
                string.Empty,
                block.Command,
                _emptyProperties);
            var table = reader.ToDataSet().Tables[0];
            var result = new TableResult(isScalarCapture, table);

            return result;
        }
    }
}
