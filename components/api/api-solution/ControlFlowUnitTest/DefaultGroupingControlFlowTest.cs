using System;
using System.Linq;
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
            Assert.Empty(declaration.GroupingContent.GroupingItems[0].IngestCommand!.Properties);
            Assert.Equal(
                "set",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Command);
            Assert.Equal(
                "sampleTable",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Table);
            Assert.Equal(
                "datatable(name:string) ['Alice', 'Bob']",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Query);
        }

        [Fact]
        public void OneIngestCommandWithProperties()
        {
            var text = @"@control-flow{
                .set sampleTable with (creationTime='2017-02-13T11:09:36.7992775Z')
                    <| datatable(name:string) ['Alice', 'Bob']
            }";
            var declaration = ParseDeclaration(text);

            Assert.Empty(declaration.Properties);
            Assert.Single(declaration.GroupingContent.GroupingItems);
            Assert.NotNull(declaration.GroupingContent.GroupingItems[0].IngestCommand);
            Assert.Single(declaration.GroupingContent.GroupingItems[0].IngestCommand!.Properties);
            Assert.Equal(
                "creationTime",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Properties[0].Id);
            Assert.Equal(
                "2017-02-13T11:09:36.7992775Z",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Properties[0].String);
            Assert.Equal(
                "set",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Command);
            Assert.Equal(
                "sampleTable",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Table);
            Assert.Equal(
                "datatable(name:string) ['Alice', 'Bob']",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Query);
        }

        [Fact]
        public void TwoIngestCommands()
        {
            var text = @"@control-flow{
                .set sampleTable <| datatable(name:string) ['Alice', 'Bob'];
                .append sampleTable2 <| sampleTable
            }";
            var declaration = ParseDeclaration(text);

            Assert.Empty(declaration.Properties);
            Assert.Equal(2, declaration.GroupingContent.GroupingItems.Length);

            Assert.NotNull(declaration.GroupingContent.GroupingItems[0].IngestCommand);
            Assert.Empty(declaration.GroupingContent.GroupingItems[0].IngestCommand!.Properties);
            Assert.Equal(
                "set",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Command);
            Assert.Equal(
                "sampleTable",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Table);
            Assert.Equal(
                "datatable(name:string) ['Alice', 'Bob']",
                declaration.GroupingContent.GroupingItems[0].IngestCommand!.Query);

            Assert.NotNull(declaration.GroupingContent.GroupingItems[1].IngestCommand);
            Assert.Empty(declaration.GroupingContent.GroupingItems[1].IngestCommand!.Properties);
            Assert.Equal(
                "append",
                declaration.GroupingContent.GroupingItems[1].IngestCommand!.Command);
            Assert.Equal(
                "sampleTable2",
                declaration.GroupingContent.GroupingItems[1].IngestCommand!.Table);
            Assert.Equal(
                "sampleTable",
                declaration.GroupingContent.GroupingItems[1].IngestCommand!.Query);
        }
    }
}