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
            Assert.NotNull(statement.Command.ShowProcedureRunSteps);
        }
    }
}