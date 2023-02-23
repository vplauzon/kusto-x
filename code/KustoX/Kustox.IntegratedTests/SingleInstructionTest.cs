using Kustox.Compiler;
using Kustox.Runtime;

namespace Kustox.IntegratedTests
{
    public class SingleInstructionTest : TestBase
    {
        [Fact]
        public async Task Empty()
        {
            var script = @"@control-flow{  }";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(controlFlow, CancellationToken.None);

            var runtime = new ControlFlowRuntime(flowInstance);

            await runtime.RunAsync();
        }

        [Fact]
        public async Task CapturePrint()
        {
            var script = @"@control-flow{
    @capture-scalar myConstant = print 2
}";
            var controlFlow = new KustoxCompiler().CompileScript(script);
            var flowInstance = CreateControlFlowInstance();

            Assert.NotNull(controlFlow);
            await flowInstance.CreateInstanceAsync(controlFlow, CancellationToken.None);

            var runtime = new ControlFlowRuntime(flowInstance);

            await runtime.RunAsync();
        }
    }
}