using Kustox.Compiler;

namespace Kustox.UnitTests.Commands
{
    public class DeleteTest
    {
        [Fact]
        public void Vanila()
        {
            var script = @$"
.delete table MyTable records <|
    MyTable
    | where Colour==""Blue""
";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.DeleteCommand);
            Assert.Equal("MyTable", statement.Command.DeleteCommand.TableName);
            Assert.Contains("Blue", statement.Command.DeleteCommand.Query?.Code);
        }
    }
}