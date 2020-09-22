using CoreDataFlow;
using System;
using Xunit;

namespace DataFlowUnitTest.Grammar
{
    public class GrammarTest
    {
        [Fact]
        public void Empty()
        {
            var test = @"dataflow
            {
            }";
         
            TestText(test);
        }

        [Fact]
        public void EmptySequence()
        {
            var test = @"dataflow
            {
                sequence
                {
                }
            }";

            TestText(test);
        }

        [Fact]
        public void EmptyParallel()
        {
            var test = @"dataflow
            {
                parallel
                {
                }
            }";

            TestText(test);
        }

        private static void TestText(string test)
        {
            var match = GrammarSingleton.Instance.Match("main", test);

            Assert.NotNull(match);
        }
    }
}