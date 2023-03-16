﻿namespace Kustox.Compiler
{
    public class GetBlobDeclaration : DeclarationBase
    {
        public string RootUrl { get; set; } = string.Empty;

        internal override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(RootUrl))
            {
                throw new InvalidDataException("RootUrl unspecified");
            }
        }
    }
}