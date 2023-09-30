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
    public class StreamingBufferTest : TestBase
    {
        [Fact]
        public async Task SingleAppend()
        {
            var store = StorageHub.ProcedureRunStore;
            var ct = new CancellationToken();
            var run = new ProcedureRun($"test-run", ProcedureRunState.Completed, DateTime.Now);

            await store.AppendRunAsync(new[] { run }, ct);
        }

        [Fact]
        public async Task TwoSequentialAppends()
        {
            var store = StorageHub.ProcedureRunStore;
            var ct = new CancellationToken();
            var run1 = new ProcedureRun($"test-run-1", ProcedureRunState.Completed, DateTime.Now);
            var run2 = new ProcedureRun($"test-run-2", ProcedureRunState.Completed, DateTime.Now);

            await store.AppendRunAsync(new[] { run1 }, ct);
            await store.AppendRunAsync(new[] { run2 }, ct);
        }

        [Fact]
        public async Task MultipleParallelAppends()
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