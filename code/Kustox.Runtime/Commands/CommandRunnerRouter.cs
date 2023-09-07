using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
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
        private readonly CommandRunnerBase _generic;
        private readonly CommandRunnerBase _getBlobs;

        public CommandRunnerRouter(ConnectionProvider connectionProvider)
        {
            _generic = new GenericCommandRunner(connectionProvider);
            _getBlobs = new GetBlobsCommandRunner(connectionProvider);
        }

        public async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct)
        {
            if (command.GenericCommand != null)
            {
                return await _generic.RunCommandAsync(command, ct);
            }
            else if (command.GetBlobs != null)
            {
                return await _getBlobs.RunCommandAsync(command, ct);
            }
            else if (command.RunProcedureCommand != null)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException($"Command type");
            }
        }
    }
}