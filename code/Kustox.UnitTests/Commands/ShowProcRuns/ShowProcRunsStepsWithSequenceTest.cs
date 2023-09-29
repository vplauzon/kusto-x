using Kustox.Compiler;

namespace Kustox.UnitTests.Commands.ShowProcRuns
{
    public class ShowProcRunsStepsWithSequenceTest
    {
        [Fact]
        public void Vanila()
        {
            var script = @".show procedure runs 'abc' steps [1,2,3]";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.JobId);
            Assert.Equal("abc", statement.Command.ShowProcedureRuns.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Breadcrumb);
            Assert.True(statement.Command.ShowProcedureRuns.Breadcrumb
                .SequenceEqual(new[] {1,2,3}));
            Assert.True(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.False(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }

        [Fact]
        public void WithQuery()
        {
            var script = @".show procedure runs 'zerun' steps [1,2,3] | project A='def'";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRuns);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Query);
            Assert.Contains("project", statement.Command.ShowProcedureRuns.Query.Code);
            Assert.NotNull(statement.Command.ShowProcedureRuns.JobId);
            Assert.Equal("zerun", statement.Command.ShowProcedureRuns.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRuns.Breadcrumb);
            Assert.True(statement.Command.ShowProcedureRuns.Breadcrumb
                .SequenceEqual(new[] { 1, 2, 3 }));
            Assert.True(statement.Command.ShowProcedureRuns.IsSteps);
            Assert.False(statement.Command.ShowProcedureRuns.IsResult);
            Assert.False(statement.Command.ShowProcedureRuns.IsHistory);
            Assert.False(statement.Command.ShowProcedureRuns.IsChildren);
        }
    }
}