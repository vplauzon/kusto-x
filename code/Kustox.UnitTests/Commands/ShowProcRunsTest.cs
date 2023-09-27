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
        }
    }
}