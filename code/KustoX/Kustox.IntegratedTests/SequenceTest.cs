using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;

namespace Kustox.IntegratedTests
{
    public class SequenceTest : TestBase
    {
        [Fact]
        public async Task Capture2Prints()
        {
            var script = @"@control-flow{
    @capture-scalar myConstant = print 2

    @capture-scalar myConstant2 = print dynamic({'Name':'Test', 'Quantity':45.4})
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);
            await RunInPiecesAsync(flowInstance);
        }

        [Fact]
        public async Task CaptureThenUseTable()
        {
            var script = @"@control-flow{
    @capture myVersion = .show version

    @capture-scalar myConstant = myVersion | project ServiceType
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);
            await RunInPiecesAsync(flowInstance);
        }
    }
}