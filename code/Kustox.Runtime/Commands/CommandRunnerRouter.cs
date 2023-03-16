using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
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
            _commandTypeToRunnerMap = builder.ToImmutableDictionary();
        }

        public async Task<TableResult> RunCommandAsync(
            ExtendedCommandType commandType,
            string commandScript,
            bool isScalarCapture,
            CancellationToken ct)
        {
            if (_commandTypeToRunnerMap.TryGetValue(commandType, out var runner))
            {
                return await runner.RunCommandAsync(commandScript, isScalarCapture, ct);
            }
            else
            {
                throw new NotSupportedException($"Command type '{commandType}'");
            }
        }
    }
}