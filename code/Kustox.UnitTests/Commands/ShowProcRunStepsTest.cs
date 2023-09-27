using Kustox.Compiler;

namespace Kustox.UnitTests.Commands
{
    public class ShowProcRunStepsTest
    {
        [Fact]
        public void Vanila()
        {
            var script = @".show procedure run 'abc' steps";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRunsSteps);
            Assert.Equal("abc", statement.Command.ShowProcedureRunsSteps.JobId);
        }

        [Fact]
        public void WithQuery()
        {
            var script = @".show procedure run 'abc' steps | project A='def'";
            var statement = new KustoxCompiler().CompileStatement(script);

            Assert.NotNull(statement);
            Assert.NotNull(statement.Command);
            Assert.NotNull(statement.Command.ShowProcedureRunsSteps);
            Assert.Equal("abc", statement.Command.ShowProcedureRunsSteps.JobId);
            Assert.NotNull(statement.Command.ShowProcedureRunsSteps.Query);
            Assert.Contains("project A='def'", statement.Command.ShowProcedureRunsSteps.Query.Code);
        }
    }
}