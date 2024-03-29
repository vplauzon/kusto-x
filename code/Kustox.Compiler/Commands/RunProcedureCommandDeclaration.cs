﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Compiler.Commands
{
    public class RunProcedureCommandDeclaration : DeclarationBase
    {
        public SequenceDeclaration RootSequence { get; set; } = new SequenceDeclaration();

        internal override void Validate()
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