namespace ControlFlow.LanguageModel
{
    public class GroupingContentDeclaration
    {
        public static GroupingContentDeclaration Empty { get; } = new GroupingContentDeclaration();

        public SnapshotDeclaration[] Snapshots { get; set; } = SnapshotDeclaration.Empty;

        public GroupingItem[] GroupingItems { get; set; } = GroupingItem.Empty;
    }
}