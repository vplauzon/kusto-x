using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text.Json;

namespace Kustox.IntegratedTests
{
    public class IfTest : TestBase
    {
        [Fact]
        public async Task IfAndElse()
        {
            foreach (var condition in new[] { true, false })
            {
                var script = $@"{{
    @capture-scalar myCondition = print {condition}

    @if myCondition {{
        print ""First""

    }}
    @else {{
        print ""Second""

    }}
}}";
                var output = await RunInPiecesAsync(script);

                Assert.NotNull(output.Result);
                Assert.False(output.Result.IsScalar);
                Assert.Single(output.Result.Columns);
                Assert.Single(output.Result.Data);
                Assert.Single(output.Result.Data.First());
                Assert.Equal(condition ? "First" : "Second", output.Result.Data[0][0]);
            }
        }
    }
}
