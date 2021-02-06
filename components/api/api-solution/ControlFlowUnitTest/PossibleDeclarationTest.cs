using ControlFlow;
using System;
using Xunit;

namespace ControlFlowUnitTest
{
    public class PossibleDeclarationTest
    {
        [Fact]
        public void EmptyGrouping()
        {
            var test = @"@control-flow{
                @grouping{
                }
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyGroupingWithOnePropertyOnFlow()
        {
            var test = @"@control-flow with (a=4){
                @grouping{
                }
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyGroupingWithOnePropertiesOnBoth()
        {
            var test = @"@control-flow with (a=4, a=true){
                @grouping with (concurrency=2){
                }
            }";

            AssertDeclaration(test);
        }

        private static void AssertDeclaration(string text)
        {
            var declaration = LanguageParser.ParseDeclaration(text);

            Assert.NotNull(declaration);
        }
    }
}