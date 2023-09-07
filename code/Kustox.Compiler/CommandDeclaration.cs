using Kusto.Language;
using Kusto.Language.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Compiler
{
    public class CommandDeclaration : DeclarationBase
    {
        public RunProcedureCommandDeclaration? RunProcedureCommand { get; set; }

        public GetBlobDeclaration? GetBlobs { get; set; }

        public GenericCommandDeclaration? GenericCommand { get; set; }

        internal override void Validate()
        {
            base.Validate();

            var commandCount = (RunProcedureCommand == null ? 0 : 1)
                + (GetBlobs == null ? 0 : 1)
                + (GenericCommand == null ? 0 : 1);

            if (commandCount != 1)
            {
                throw new InvalidDataException(
                    "Must have one and only one command in"
                    + $" {typeof(CommandDeclaration).Name}");
            }
            RunProcedureCommand?.Validate();
            GetBlobs?.Validate();
            GenericCommand?.Validate();
        }
    }
}