using Kusto.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Compiler
{
    public class CommandDeclaration : DeclarationCodeBase
    {
        public ExtendedCommandType CommandType { get; set; }

        public GetBlobDeclaration? GetBlobs { get; set; }

        internal override void SubParsing(KustoxCompiler compiler)
        {
            base.SubParsing(compiler);

            var block = compiler.ParseCommand(Code);

            CommandType = block.CommandType;
            GetBlobs = block.GetBlobs;
        }
    }
}