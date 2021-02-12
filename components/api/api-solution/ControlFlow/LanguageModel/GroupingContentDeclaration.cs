namespace ControlFlow.LanguageModel
{
    public class GroupingContentDeclaration
    {
        public static GroupingContentDeclaration Empty { get; } = new GroupingContentDeclaration();

        public GroupingItem[] GroupingItems { get; set; } = GroupingItem.Empty;
    }
}