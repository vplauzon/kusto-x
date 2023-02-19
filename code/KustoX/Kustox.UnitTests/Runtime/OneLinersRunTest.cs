using Kustox.Compiler;
using Kustox.Runtime;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.UnitTests.Runtime
{
    public class OneLinersRunTest
    {
        [Fact]
        public async Task Empty()
        {
            var script = @"@control-flow{  }";
            var controlFlowDeclaration = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlowDeclaration);

            var jobId = 42;
            var mockPersistency = new Mock<IControlFlowPersistency>();

            mockPersistency
                .Setup(p => p.GetControlFlowDeclarationAsync(jobId))
                .Returns(Task.FromResult(controlFlowDeclaration));

            var runtime = new ControlFlowRuntime(jobId, mockPersistency.Object);

            await runtime.RunAsync();
        }
    }
}
