using Kustox.Compiler;

namespace Kustox.UnitTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsResultTest
    {
        [Fact]
        public void Vanila()
        {
            var script = @".show procedure runs 'abc' steps [1,2,3] result";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.JobId);
            Assert.Equal("abc", statement.Command.ShowProcedureRuns.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Breadcrumb);
            Assert.True(statement.Command.ShowProcedureRuns.Breadcrumb
                .SequenceEqual(new[] { 1, 2, 3 }));
            Assert.True(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.True(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }

        [Fact]
        public void WithQuery()
        {
            var script = @".show procedure runs 'don' steps [1,2,3] result | project A='def'";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Query);
            Assert.Contains("project", statement.Command.ShowProcedureRuns.Query.Code);
            Assert.NotNull(statement.Command.ShowProcedureRuns.JobId);
            Assert.Equal("don", statement.Command.ShowProcedureRuns.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Breadcrumb);
            Assert.True(statement.Command.ShowProcedureRuns.Breadcrumb
                .SequenceEqual(new[] { 1, 2, 3 }));
            Assert.True(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.True(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }
    }
}