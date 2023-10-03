using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsChildrenTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var procScript = @"{
    @capture names = datatable(name:string) [""Alice"", ""Bob""]

    @foreach(name in names) with(concurrency=2) {
        print name=name

    }

}";
            var output = await RunInPiecesAsync(procScript, null);
            var jobId = output.JobId;
            var showScript0 = $".show proc runs '{jobId}' steps [0] children";
            var showScript1 = $".show proc runs '{jobId}' steps [1] children";
            var result0 = await RunStatementAsync(showScript0);
            var result1 = await RunStatementAsync(showScript1);

            Assert.False(result0.IsScalar);
            Assert.Single(result0.Data);
            Assert.Equal(StepState.Completed.ToString(), result0.GetColumnData("State").First());

            Assert.False(result1.IsScalar);
            Assert.Equal(3, result1.Data.Count);
            Assert.Empty(result1
                .GetColumnData("State")
                .Where(s => (string)s! != StepState.Completed.ToString()));
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