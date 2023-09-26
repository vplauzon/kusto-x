using Kustox.Compiler;
using Kustox.Runtime;
using Kustox.Runtime.State;
using System.Collections.Immutable;

namespace Kustox.IntegratedTests.Commands
{
    public class GetBlobsTest : TestBase
    {
        [Fact]
        public async Task ThreeFiles()
        {
            var url = $"{SampleRootUrl}/3-files/";
            var script = @$"{{
    .get blobs '{url}'
}}";
            var result = await RunInPiecesAsync(script);

            Assert.NotNull(result);
            Assert.False(result.IsScalar);
            Assert.Equal(3, result.Columns.Count());
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task ThreeFilesNames()
        {
            var url = $"{SampleRootUrl}/3-files/";
            var script = @$"{{
    @capture blobs = .get blobs '{url}'
    
    blobs
    | project Name
}}";
            var result = await RunInPiecesAsync(script);

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