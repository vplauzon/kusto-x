using ControlFlow;
using System;
using Xunit;

namespace ControlFlowUnitTest
{
    public class PossibleDeclarationTest
    {
        #region Empty flows
        [Fact]
        public void EmptyDataflow()
        {
            var test = @"@control-flow{
            }";
         
            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyDataflowWithNoProperty()
        {
            var test = @"@control-flow with (){
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyDataflowWithOneProperty()
        {
            var test = @"@control-flow with (a=4){
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyDataflowWithManyProperties()
        {
            var test = @"@control-flow with (a=4, ab=true, Z14b=46){
            }";

            AssertDeclaration(test);
        }

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
        #endregion

        private static void AssertDeclaration(string text)
        {
            var declaration = LanguageParser.ParseDeclaration(text);

            Assert.NotNull(declaration);
        }
    }
}