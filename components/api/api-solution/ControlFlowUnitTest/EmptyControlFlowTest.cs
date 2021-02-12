using System;
using Xunit;

namespace ControlFlowUnitTest
{
    public class EmptyControlFlowTest : ControlFlowTestBase
    {
        [Fact]
        public void EmptyControlflow()
        {
            var text = @"@control-flow{
            }";
            var declaration = ParseDeclaration(text);

            Assert.Empty(declaration.Properties);
            Assert.Empty(declaration.GroupingContent.Snapshots);
            Assert.Empty(declaration.GroupingContent.GroupingItems);
        }

        [Fact]
        public void EmptyDataflowWithNoProperty()
        {
            var text = @"@control-flow with (){
            }";
            var declaration = ParseDeclaration(text);

            Assert.Empty(declaration.Properties);
            Assert.Empty(declaration.GroupingContent.Snapshots);
            Assert.Empty(declaration.GroupingContent.GroupingItems);
        }

        [Fact]
        public void EmptyDataflowWithOneProperty()
        {
            var text = @"@control-flow with (a=4){
            }";
            var declaration = ParseDeclaration(text);
            var property = Assert.Single(declaration.Properties);

            Assert.Equal("a", property.Id);
            Assert.Equal(4, property.Integer);
            Assert.Empty(declaration.GroupingContent.Snapshots);
            Assert.Empty(declaration.GroupingContent.GroupingItems);
        }

        [Fact]
        public void EmptyDataflowWithManyProperties()
        {
            var text = @"@control-flow with (a=4, ab=true, Z14b=46){
            }";
            var declaration = ParseDeclaration(text);

            Assert.Equal(3, declaration.Properties.Length);
            Assert.Equal("a", declaration.Properties[0].Id);
            Assert.Equal(4, declaration.Properties[0].Integer);
            Assert.Equal("ab", declaration.Properties[1].Id);
            Assert.Equal(true, declaration.Properties[1].Boolean);
            Assert.Equal("Z14b", declaration.Properties[2].Id);
            Assert.Equal(46, declaration.Properties[2].Integer);
            Assert.Empty(declaration.GroupingContent.Snapshots);
            Assert.Empty(declaration.GroupingContent.GroupingItems);
        }
    }
}