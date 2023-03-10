namespace ControlFlow.LanguageModel
{
    public class GroupingDeclaration : PropertiesHolderBase
    {
        public static GroupingDeclaration Empty { get; } = new GroupingDeclaration();

        public GroupingContentDeclaration GroupingContent { get; set; } = GroupingContentDeclaration.Empty;
    }
}