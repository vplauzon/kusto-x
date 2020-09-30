using CoreDataFlow;
using System;
using Xunit;

namespace DataFlowUnitTest.Grammar
{
    public class GrammarTest
    {
        [Fact]
        public void EmptyDataflow()
        {
            var test = @"dataflow
            {
            }";
         
            TestText(test);
        }

        [Fact]
        public void EmptyDataflowWithNoProperty()
        {
            var test = @"dataflow with ()
            {
            }";

            TestText(test);
        }

        [Fact]
        public void EmptyDataflowWithOneProperty()
        {
            var test = @"dataflow with (a=4)
            {
            }";

            TestText(test);
        }

        [Fact]
        public void EmptyDataflowWithManyProperties()
        {
            var test = @"dataflow with (a=4, ab=true, Z14b=46)
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
        public void EmptySequenceWithOnePropertyOnFlow()
        {
            var test = @"dataflow with (a=4)
            {
                sequence
                {
                }
            }";

            TestText(test);
        }

        [Fact]
        public void EmptySequenceWithOnePropertiesOnBoth()
        {
            var test = @"dataflow with (a=4, banana=true)
            {
                sequence with (b42=false)
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
            var output = match.ComputeOutput();

            Assert.NotNull(match);
            Assert.NotNull(output);
        }
    }
}