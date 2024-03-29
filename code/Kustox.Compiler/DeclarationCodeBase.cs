﻿namespace Kustox.Compiler
{
    public abstract class DeclarationCodeBase : DeclarationBase
    {
        public string Code { get; set; } = string.Empty;

        internal override void Validate()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                throw new InvalidDataException($"No '{nameof(Code)}'");
            }
        }
    }
}