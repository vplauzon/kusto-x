using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.Commands
{
    internal class GenericCommandRunner : CommandRunnerBase
    {
        private static readonly ClientRequestProperties _emptyProperties =
            new ClientRequestProperties();

        public GenericCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            var reader = await ConnectionProvider.CommandProvider.ExecuteControlCommandAsync(
                string.Empty,
                command.GenericCommand!.Code,
                _emptyProperties);
            var table = reader.ToDataSet().Tables[0];

            return table.ToTableResult();
        }
    }
}
