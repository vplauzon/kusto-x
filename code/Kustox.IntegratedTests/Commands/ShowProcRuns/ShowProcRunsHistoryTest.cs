using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsHistoryTest : TestBase
    {
        [Fact]
        public async Task ShowVersion()
        {
            var procScript = @"{
    .show version
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' history";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.True(result.Data.Count >= 2);
            Assert.Contains(StepState.Completed.ToString(), result.GetColumnData("State"));
            Assert.Contains(StepState.Running.ToString(), result.GetColumnData("State"));
        }

        [Fact]
        public async Task ShowVersionColumn()
        {
            var procScript = @"{
    .show version
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' history | project State";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.True(result.Data.Count >= 2);
            Assert.Contains(StepState.Completed.ToString(), result.GetColumnData(0));
            Assert.Contains(StepState.Running.ToString(), result.GetColumnData(0));
        }
    }
}