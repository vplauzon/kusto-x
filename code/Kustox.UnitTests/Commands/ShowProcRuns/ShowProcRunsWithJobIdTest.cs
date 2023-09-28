using Kustox.Compiler;

namespace Kustox.UnitTests.Commands.ShowProcRuns
{
    public class ShowProcRunsWithJobIdTest
    {
        [Fact]
        public void WithJobId()
        {
            var script = @".show procedure runs 'abc'";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.JobId);
            Assert.Equal("abc", statement.Command.ShowProcedureRuns.JobId);
            Assert.Null(statement.Command.ShowProcedureRuns.Steps);
            Assert.False(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.False(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }

        [Fact]
        public void WithQueryAndJobId()
        {
            var script = @".show proc runs ""myjobid"" | take 10";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Query);
            Assert.Contains("take 10", statement.Command.ShowProcedureRuns.Query.Code);
            Assert.NotNull(statement.Command.ShowProcedureRuns.JobId);
            Assert.Equal("myjobid", statement.Command.ShowProcedureRuns.JobId);
            Assert.Null(statement.Command.ShowProcedureRuns.Steps);
            Assert.False(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.False(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }
    }
}