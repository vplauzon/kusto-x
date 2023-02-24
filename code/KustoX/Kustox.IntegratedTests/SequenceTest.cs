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

    @capture-scalar myConstant = print 3
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(controlFlow, CancellationToken.None);
            await RunInPiecesAsync(flowInstance);
        }
    }
}