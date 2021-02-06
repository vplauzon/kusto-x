using System;
using System.Collections.Generic;
using System.Text;

namespace ControlFlow.LanguageModel
{
    public class ControlFlowDeclaration : PropertiesHolderBase
    {
        public GroupingContentDeclaration GroupingContent { get; set; } = GroupingContentDeclaration.Empty;
    }
}