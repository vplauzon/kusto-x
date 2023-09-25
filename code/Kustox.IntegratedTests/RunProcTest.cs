using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.IntegratedTests
{
    public class RunProcTest : TestBase
    {
        [Fact]
        public async Task PickResult()
        {
            var ct = default(CancellationToken);
            var environmentRuntime = CreateEnvironmentRuntime();

            await environmentRuntime.StartAsync(false, ct);

            try
            {
                var script = @".run-procedure <| {
    @capture-scalar a = print 40

    @capture-scalar b = print 2;

    print a+b
}";
                var statement = Compiler.CompileStatement(script)!;
                var result = await environmentRuntime.RunnableRuntime.RunStatementAsync(
                    statement,
                    ImmutableDictionary<string, TableResult?>.Empty,
                    ct);
                var jobId = (string)result.Data[0][0];
                //  Timeout for completion:  account for running on a laptop tethering on mobile!
                var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(12));

                //  Prob until completion
                while (true)
                {
                    var run = await environmentRuntime.ProcedureRunStore.GetLatestRunAsync(
                        jobId,
                        cancelSource.Token);

                    Assert.NotNull(run);
                    if (run.State == ProcedureRunState.Completed)
                    {
                        throw new NotImplementedException();
                    }
                    Assert.True(run.State == ProcedureRunState.Running
                        || run.State == ProcedureRunState.Pending);
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
            finally
            {
                await environmentRuntime.StopAsync(ct);
            }
        }
    }
}