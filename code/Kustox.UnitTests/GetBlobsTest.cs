using Kustox.Compiler;

namespace Kustox.UnitTests
{
    public class GetBlobsTest
    {
        [Fact]
        public void WithUrl()
        {
            var url = "https://myaccount.blob.core.windows.net/mycontainer/myfolder/";
            var script = @$".run-procedure <| {{
    .get blobs '{url}'
}}";
            var controlFlow = new KustoxCompiler().CompileScript(script);

            Assert.NotNull(controlFlow);
            Assert.Single(controlFlow.RootSequence.Blocks);
            Assert.NotNull(controlFlow.RootSequence.Blocks.First().Command);
            Assert.Equal(
                ExtendedCommandType.GetBlobs,
                controlFlow.RootSequence.Blocks.First().Command!.CommandType);
        }
    }
}