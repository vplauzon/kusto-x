﻿using Kustox.Compiler.Commands;
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
        private readonly CommandRunnerBase _listBlobs;
        private readonly CommandRunnerBase _runProcedure;
        private readonly CommandRunnerBase _showProcedureRuns;
        private readonly CommandRunnerBase _append;
        private readonly CommandRunnerBase _delete;

        public CommandRunnerRouter(
            ConnectionProvider connectionProvider,
            IStorageHub storageHub,
            IProcedureQueue procedureQueue)
        {
            _generic = new GenericCommandRunner(connectionProvider);
            _listBlobs = new ListBlobsCommandRunner(connectionProvider);
            _runProcedure = new RunProcedureCommandRunner(
                connectionProvider,
                procedureQueue);
            _showProcedureRuns = new ShowProcedureRunsCommandRunner(
                connectionProvider,
                storageHub);
            _append = new AppendCommandRunner(connectionProvider);
            _delete = new DeleteCommandRunner(connectionProvider);
        }

        public async Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            if (command.GenericCommand != null)
            {
                return await _generic.RunCommandAsync(command, captures, ct);
            }
            else if (command.ListBlobsCommand != null)
            {
                return await _listBlobs.RunCommandAsync(command, captures, ct);
            }
            else if (command.RunProcedureCommand != null)
            {
                return await _runProcedure.RunCommandAsync(command, captures, ct);
            }
            else if (command.ShowProcedureRuns != null)
            {
                return await _showProcedureRuns.RunCommandAsync(command, captures, ct);
            }
            else if (command.AppendCommand != null)
            {
                return await _append.RunCommandAsync(command, captures, ct);
            }
            else if (command.DeleteCommand != null)
            {
                return await _delete.RunCommandAsync(command, captures, ct);
            }
            else
            {
                throw new NotSupportedException($"Command type");
            }
        }
    }
}