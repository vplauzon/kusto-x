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
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.RunProcedureCommand);
            Assert.Single(statement.Command.RunProcedureCommand.RootSequence.Blocks);

            var command =
                statement.Command.RunProcedureCommand.RootSequence.Blocks.First().Command;

            Assert.NotNull(command);
            Assert.NotNull(command.GetBlobsCommand);
        }
    }
}