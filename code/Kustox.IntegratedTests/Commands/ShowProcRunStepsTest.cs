using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands
{
    public class ShowProcRunStepsTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var script = ".show proc run 'abc' steps";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
        }

        [Fact]
        public async Task WithQuery()
        {
            var script = ".show procedure run 'abc' steps | project A='123'";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.NotEmpty(result.Data);
            Assert.Equal("123", result.Data[0][0]);
        }
    }
}