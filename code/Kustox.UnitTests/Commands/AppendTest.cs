using Kustox.Compiler;

namespace Kustox.UnitTests.Commands
{
    public class AppendTest
    {
        [Fact]
        public void NoProperties()
        {
            var script = @$".append MyTable <| myVar";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.AppendCommand);
            Assert.Empty(statement.Command.AppendCommand.Properties);
            Assert.Equal("MyTable", statement.Command.AppendCommand.TableName);
            Assert.Equal("myVar", statement.Command.AppendCommand.CaptureId);
        }

        [Fact]
        public void WithFolderProperty()
        {
            var script = @$".append MyTable with (folder=""myfolder"") <| myVar";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.AppendCommand);
            Assert.Single(statement.Command.AppendCommand.Properties);
            Assert.Equal("folder", statement.Command.AppendCommand.Properties.First().Id);
            Assert.Equal("myfolder", statement.Command.AppendCommand.Properties.First().String);
            Assert.Equal("MyTable", statement.Command.AppendCommand.TableName);
            Assert.Equal("myVar", statement.Command.AppendCommand.CaptureId);
        }
    }
}