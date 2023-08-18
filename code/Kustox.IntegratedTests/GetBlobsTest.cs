using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests
{
    public class GetBlobsTest : TestBase
    {
        [Fact]
        public async Task ThreeFiles()
        {
            var url = $"{StorageContainerUrl}/3-files/";
            var script = @$"@run-procedure{{
    .get blobs '{url}'
}}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateRunAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Equal(3, result.Columns.Count());
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task ThreeFilesNames()
        {
            var url = $"{StorageContainerUrl}/3-files/";
            var script = @$"@run-procedure{{
    @capture blobs = .get blobs '{url}'
    
    blobs
    | project Name
}}";
            var flowInstance = CreateControlFlowInstance();

            await flowInstance.CreateRunAsync(script, CancellationToken.None);

            var result = await RunInPiecesAsync(flowInstance);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Single(result.Columns);
            Assert.Equal(3, result.Data.Count());

            var names = result.Data
                .Select(d => d.First().ToString()!)
                .Select(n => n.Split('/').Last())
                .Order()
                .ToImmutableArray();

            Assert.Equal("a.txt", names[0]);
            Assert.Equal("b.txt", names[1]);
            Assert.Equal("c.txt", names[2]);
        }
    }
}