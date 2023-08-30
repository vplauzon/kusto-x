﻿using Kusto.Data.Common;
using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime.Commands
{
    internal abstract class CommandRunnerBase
    {
        protected CommandRunnerBase(ConnectionProvider connectionProvider)
        {
            ConnectionProvider = connectionProvider;
        }

        protected ConnectionProvider ConnectionProvider { get; }

        public abstract Task<TableResult> RunCommandAsync(
            CommandDeclaration command,
            CancellationToken ct);
    }
}