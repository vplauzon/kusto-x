using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.IntegratedTests
{
    internal class StreamingBufferTest : TestBase
    {
        public async Task MultipleAppend()
        {
            var store = StorageHub.ProcedureRunStore;
            var ct = new CancellationToken();
            //  Run 10 appends in parallel
            var tasks = Enumerable.Range(0, 10)
                .Select(i => new ProcedureRun(
                    $"test-{i}",
                    ProcedureRunState.Completed,
                    DateTime.Now))
                .Select(run => Task.Run(async () =>
                {
                    await store.AppendRunAsync(new[] { run }, ct);
                }))
                .ToImmutableArray();

            await Task.WhenAll(tasks);
        }
    }
}