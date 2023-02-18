using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Compiler
{
    public class ControlFlowDeclaration : DeclarationBase
    {
        public ControlFlowDeclaration() : base(false)
        {
        }

        public GroupingDeclaration RootGrouping { get; set; } = new GroupingDeclaration();

        public override void Validate()
        {
            base.Validate();

            if (RootGrouping == null)
            {
                throw new InvalidDataException($"No '{nameof(RootGrouping)}'");
            }
            RootGrouping.Validate();
        }
    }
}