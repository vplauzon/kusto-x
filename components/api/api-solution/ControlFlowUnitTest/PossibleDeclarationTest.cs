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
            var test = @"control-flow{
            }";
         
            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyDataflowWithNoProperty()
        {
            var test = @"control-flow with (){
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyDataflowWithOneProperty()
        {
            var test = @"control-flow with (a=4){
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyDataflowWithManyProperties()
        {
            var test = @"control-flow with (a=4, ab=true, Z14b=46){
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptySequence()
        {
            var test = @"control-flow{
                sequence{
                }
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptySequenceWithOnePropertyOnFlow()
        {
            var test = @"control-flow with (a=4){
                sequence{
                }
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptySequenceWithOnePropertiesOnBoth()
        {
            var test = @"control-flow with (a=4, banana=true){
                sequence with (b42=false){
                }
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void EmptyParallel()
        {
            var test = @"control-flow{
                parallel{
                }
            }";

            AssertDeclaration(test);
        }
        #endregion

        [Fact]
        public void SequenceWithOneCommand()
        {
            var test = @"control-flow{
                sequence{
                    .set-or-replace T <|
                        datatable (name:string) ['Alice', 'Bob']
                }
            }";

            AssertDeclaration(test);
        }

        [Fact]
        public void ParallelWithTwoCommands()
        {
            var test = @"control-flow{
                sequence{
                    .set-or-replace T <|
                        datatable (name:string) ['Alice', 'Bob']

                    .append Table123 <|
                        TableXyz
                        | summarize count() by name
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