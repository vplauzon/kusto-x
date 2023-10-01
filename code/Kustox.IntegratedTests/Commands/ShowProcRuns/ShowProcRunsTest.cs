using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests.Commands.ShowProcRuns
{
    public class ShowProcRunsTest : TestBase
    {
        [Fact]
        public async Task Vanila()
        {
            var script = ".show procedure runs";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
        }

        [Fact]
        public async Task WithQuery()
        {
            var script = ".show procedure runs | summarize take_any (*) | project A='123'";
            var result = await RunStatementAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.NotEmpty(result.Data);
            Assert.Equal("123", result.Data[0][0]);
        }
    }
}