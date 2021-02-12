namespace ControlFlow.LanguageModel
{
    public class GroupingItem
    {
        public static GroupingItem[] Empty { get; } = new GroupingItem[0];

        public SnapshotDeclaration? Snapshot { get; set; } = null;
        
        public IngestCommand? IngestCommand { get; set; } = null;
    }
}