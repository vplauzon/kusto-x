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
            var output = await RunInPiecesAsync(script);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Equal(3, output.Result.Columns.Count());
            Assert.Equal(3, output.Result.Data.Count());
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
            var output = await RunInPiecesAsync(script);

            Assert.NotNull(output.Result);
            Assert.False(output.Result.IsScalar);
            Assert.Single(output.Result.Columns);
            Assert.Equal(3, output.Result.Data.Count());

            var names = output.Result.Data
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