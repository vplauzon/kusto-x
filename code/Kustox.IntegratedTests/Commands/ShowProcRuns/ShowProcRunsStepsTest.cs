using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var procScript = @"{
    print 1

    print 2

    print 3
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Equal(4, result.Data.Count);
            Assert.Equal(4, result
                .GetColumnData("State")
                .Where(s => (string)s! == StepState.Completed.ToString())
                .Count());
        }

        [Fact]
        public async Task WithQuery()
        {
            var procScript = @"{
    print 1

    print 2

    print 3
}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript = $".show proc runs '{jobId}' steps | project State";
            var result = await RunStatementAsync(showScript);

            Assert.False(result.IsScalar);
            Assert.Equal(4, result.Data.Count);
            Assert.Equal(4, result
                .GetColumnData(0)
                .Where(s => (string)s! == StepState.Completed.ToString())
                .Count());
        }
    }
}