using Kusto.Cloud.Platform.Data;
using Kustox.Compiler.Commands;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;

namespace Kustox.Runtime.Commands
{
    internal class AppendCommandRunner : CommandRunnerBase
    {
        public AppendCommandRunner(ConnectionProvider connectionProvider)
            : base(connectionProvider)
        {
        }

        public override async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            var commandText = @$"
.append {command.AppendCommand!.TableName} <|
";
            var reader = await ConnectionProvider.CommandProvider.ExecuteControlCommandAsync(
                string.Empty,
                commandText);
            var table = reader.ToDataSet().Tables[0];

            return table.ToTableResult();
        }
    }
}