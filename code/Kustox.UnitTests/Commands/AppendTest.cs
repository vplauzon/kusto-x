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
            Assert.Equal("MyTable", statement.Command.AppendCommand.TableName);
            Assert.Equal("myVar", statement.Command.AppendCommand.CaptureId);
        }
    }
}