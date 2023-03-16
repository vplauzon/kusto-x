using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests
{
    public class GetBlobsTest : TestBase
    {
        [Fact]
        public async Task RangeEmptySequence()
        {
            var url = $"{StorageContainerUrl}/3-files";
            var script = @$"@run-procedure{{
    .get blobs '{url}'
}}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateInstanceAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Single(result.Data);
            Assert.Single(result.Data.First());
        }
    }
}