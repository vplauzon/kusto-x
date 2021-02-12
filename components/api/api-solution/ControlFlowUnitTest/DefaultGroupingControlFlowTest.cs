using System;
using Xunit;

namespace ControlFlowUnitTest
{
    public class DefaultGroupingControlFlowTest : ControlFlowTestBase
    {
        [Fact]
        public void OneIngestCommand()
        {
            var text = @"@control-flow{
                .set sampleTable <| datatable(name:string) ['Alice', 'Bob']
            }";
            var declaration = ParseDeclaration(text);

            Assert.Empty(declaration.Properties);
            Assert.Single(declaration.GroupingContent.GroupingItems);
            Assert.NotNull(declaration.GroupingContent.GroupingItems[0].IngestCommand);
        }
    }
}