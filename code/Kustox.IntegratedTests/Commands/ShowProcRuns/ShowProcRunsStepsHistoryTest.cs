using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsHistoryTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var procScript = @"{
    print 'a'

    print 'b'

    print 'c'
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps [1] history";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.True(result.Data.Count >= 2);
            Assert.Contains(StepState.Completed.ToString(), result.GetColumnData("State"));
            Assert.Contains(StepState.Running.ToString(), result.GetColumnData("State"));
        }

        [Fact]
        public async Task WithQuery()
        {
            var procScript = @"{
    print 'a'

    print 'b'

    print 'c'
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps [1] history | project State";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.True(result.Data.Count >= 2);
            Assert.Contains(StepState.Completed.ToString(), result.GetColumnData(0));
            Assert.Contains(StepState.Running.ToString(), result.GetColumnData(0));
        }
    }
}