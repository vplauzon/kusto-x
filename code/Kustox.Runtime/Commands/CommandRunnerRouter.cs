using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.Commands
{
    internal class CommandRunnerRouter
    {
        private readonly IImmutableDictionary<ExtendedCommandType, CommandRunnerBase>
            _commandTypeToRunnerMap;

        public CommandRunnerRouter(ConnectionProvider connectionProvider)
        {
            var builder = ImmutableDictionary<ExtendedCommandType, CommandRunnerBase>
                .Empty
                .ToBuilder();

            builder.Add(ExtendedCommandType.Kusto, new KustoCommandRunner(connectionProvider));
            builder.Add(ExtendedCommandType.GetBlobs, new GetBlobsCommandRunner(connectionProvider));
            _commandTypeToRunnerMap = builder.ToImmutableDictionary();
        }

        public async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            if (_commandTypeToRunnerMap.TryGetValue(command.CommandType, out var runner))
            {
                return await runner.RunCommandAsync(command, ct);
            }
            else
            {
                throw new NotSupportedException($"Command type '{command.CommandType}'");
            }
        }
    }
}