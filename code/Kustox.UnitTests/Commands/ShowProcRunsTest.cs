using Kustox.Compiler;

namespace Kustox.UnitTests.Commands
{
    public class ShowProcRunsTest
    {
        [Fact]
        public void LongForm()
        {
            var script = @".show procedure runs";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
        }

        [Fact]
        public void ShortForm()
        {
            var script = @".show proc runs";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
        }
    }
}