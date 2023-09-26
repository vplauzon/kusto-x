using Kusto.Language;
using Kusto.Language.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Compiler.Commands
{
    public class CommandDeclaration : DeclarationBase
    {
        public GenericCommandDeclaration? GenericCommand { get; set; }

        public RunProcedureCommandDeclaration? RunProcedureCommand { get; set; }

        public ShowProcedureRunsCommandDeclaration? ShowProcedureRuns { get; set; }

        public ShowProcedureRunStepsCommandDeclaration? ShowProcedureRunSteps { get; set; }

        public GetBlobDeclaration? GetBlobsCommand { get; set; }

        internal override void Validate()
        {
            base.Validate();

            var commandCount = (RunProcedureCommand == null ? 0 : 1)
                + (ShowProcedureRuns == null ? 0 : 1)
                + (ShowProcedureRunSteps == null ? 0 : 1)
                + (GetBlobsCommand == null ? 0 : 1)
                + (GenericCommand == null ? 0 : 1);

            if (commandCount != 1)
            {
                throw new InvalidDataException(
                    "Must have one and only one command in"
                    + $" {typeof(CommandDeclaration).Name}");
            }
            RunProcedureCommand?.Validate();
            ShowProcedureRuns?.Validate();
            ShowProcedureRunSteps?.Validate();
            GetBlobsCommand?.Validate();
            GenericCommand?.Validate();
        }
    }
}