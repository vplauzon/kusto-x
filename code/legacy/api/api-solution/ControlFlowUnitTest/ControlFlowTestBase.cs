using ControlFlow;
using ControlFlow.LanguageModel;
using Xunit;

namespace ControlFlowUnitTest
{
    public class ControlFlowTestBase
    {
        protected static ControlFlowDeclaration ParseDeclaration(string text)
        {
            var declaration = LanguageParser.ParseExtendedCommand(text);

            Assert.NotNull(declaration);
            Assert.NotNull(declaration!.AdHocControlFlow);

            return declaration.AdHocControlFlow!;
        }
    }
}