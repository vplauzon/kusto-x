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
            Assert.NotNull(statement.Command.ShowProcedureRunsStepsResult);
            Assert.Equal("abc", statement.Command.ShowProcedureRunsStepsResult.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRunsStepsResult.Steps);
            Assert.True(statement.Command.ShowProcedureRunsStepsResult.Steps.SequenceEqual(new[] { 1, 2, 3 }));
        }

        [Fact]
        public void WithQuery()
        {
            var script = @".show procedure runs 'abc' steps [1,2,3] result | project A='def'";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRunsStepsResult);
            Assert.Equal("abc", statement.Command.ShowProcedureRunsStepsResult.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRunsStepsResult.Steps);
            Assert.True(statement.Command.ShowProcedureRunsStepsResult.Steps.SequenceEqual(new[] { 1, 2, 3 }));
            Assert.NotNull(statement.Command.ShowProcedureRunsStepsResult.Query);
            Assert.Contains("project A='def'", statement.Command.ShowProcedureRunsStepsResult.Query.Code);
        }
    }
}