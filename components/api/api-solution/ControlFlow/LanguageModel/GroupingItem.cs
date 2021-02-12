namespace ControlFlow.LanguageModel
{
    public class GroupingItem
    {
        public static GroupingItem[] Empty { get; } = new GroupingItem[0];

        public IngestCommand? IngestCommand { get; set; } = null;
    }
}