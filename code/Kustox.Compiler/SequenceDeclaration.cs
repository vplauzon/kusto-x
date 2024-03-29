﻿using Kustox.Compiler.Commands;
using System.Collections.Immutable;

namespace Kustox.Compiler
{
    public class SequenceDeclaration : DeclarationCodeBase
    {
        public IImmutableList<BlockDeclaration> Blocks { get; set; }
            = new ImmutableArray<BlockDeclaration>();

        internal override void Validate()
        {
            base.Validate();

            foreach (var block in Blocks)
            {
                block.Validate();
            }
        }
    }
}