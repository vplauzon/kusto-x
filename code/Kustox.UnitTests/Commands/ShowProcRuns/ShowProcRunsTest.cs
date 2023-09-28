using Kustox.Compiler;

namespace Kustox.UnitTests.Commands.ShowProcRuns
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
            Assert.Null(statement.Command.ShowProcedureRuns.JobId);
            Assert.Null(statement.Command.ShowProcedureRuns.Steps);
            Assert.False(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.False(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }

        [Fact]
        public void ShortForm()
        {
            var script = @".show proc runs";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.Null(statement.Command.ShowProcedureRuns.JobId);
            Assert.Null(statement.Command.ShowProcedureRuns.Steps);
            Assert.False(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.False(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }

        [Fact]
        public void WithQuery()
        {
            var script = @".show proc runs | take 10";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Query);
            Assert.Contains("take 10", statement.Command.ShowProcedureRuns.Query.Code);
        }
    }
}