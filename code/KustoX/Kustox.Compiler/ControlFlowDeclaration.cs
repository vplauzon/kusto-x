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

        public SequenceDeclaration RootSequence { get; set; } = new SequenceDeclaration();

        public override void Validate()
        {
            base.Validate();

            if (RootSequence == null)
            {
                throw new InvalidDataException($"No '{nameof(RootSequence)}'");
            }
            RootSequence.Validate();
        }
    }
}