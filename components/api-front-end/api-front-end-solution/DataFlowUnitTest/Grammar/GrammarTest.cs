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
            var match = GrammarSingleton.Instance.Match("main", test);
        }
    }
}