using Kusto.Data.Linq;
using Kustox.Compiler;
using Kustox.Runtime;

namespace Kustox.IntegratedTests
{
    public class SingleInstructionTest : TestBase
    {
        [Fact]
        public async Task Empty()
        {
            var script = @"@run-procedure{  }";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var runtime = new ProcedureRuntime(flowInstance, RunnableRuntime);

            await runtime.RunAsync();
        }

        [Fact]
        public async Task CapturePrint()
        {
            var script = @"@run-procedure{
    @capture-scalar myConstant = print 2
}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var runtime = new ProcedureRuntime(flowInstance, RunnableRuntime);

            await runtime.RunAsync();
        }

        [Fact]
        public async Task CaptureShowVersion()
        {
            var script = @"@run-procedure{
    @capture myVersion = .show version
}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var runtime = new ProcedureRuntime(flowInstance, RunnableRuntime);

            await runtime.RunAsync();
        }

        [Fact]
        public async Task NoCaptureShowVersion()
        {
            var script = @"@run-procedure{
    .show version
}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var runtime = new ProcedureRuntime(flowInstance, RunnableRuntime);

            await runtime.RunAsync();
        }
    }
}