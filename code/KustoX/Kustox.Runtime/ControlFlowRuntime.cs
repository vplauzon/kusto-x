﻿using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Xml.Linq;

namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly IControlFlowInstance _controlFlowInstance;

        public ControlFlowRuntime(IControlFlowInstance controlFlowInstance)
        {
            _controlFlowInstance = controlFlowInstance;
        }

        public async Task RunAsync(CancellationToken ct = default(CancellationToken))
        {
            var runtimeContext = await RuntimeContext.LoadContextAsync(_controlFlowInstance, ct);

            await RunGroupingAsync(
                runtimeContext.Declaration.RootGrouping,
                runtimeContext,
                ct);
        }

        private async Task RunGroupingAsync(
            GroupingDeclaration grouping,
            RuntimeContext runtimeContext,
            CancellationToken ct)
        {
            await runtimeContext.EnsureStepsAsync(grouping.Blocks.Count(), ct);
         
            foreach (var instruction in grouping.Blocks)
            {
            }
        }
    }
}