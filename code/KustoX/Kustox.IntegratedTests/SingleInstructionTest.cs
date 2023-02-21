using Kustox.Compiler;

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
            await flowInstance.CreateInstanceAsync(controlFlow);
        }
    }
}