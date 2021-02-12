namespace ControlFlow.LanguageModel
{
    public class IngestCommand : PropertiesHolderBase
    {
        public string Command { get; set; } = string.Empty;

        public string Table { get; set; } = string.Empty;

        public string Query { get; set; } = string.Empty;
    }
}