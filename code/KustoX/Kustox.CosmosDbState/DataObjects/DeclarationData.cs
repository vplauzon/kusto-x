using Kustox.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.CosmosDbState.DataObjects
{
    internal class DeclarationData
    {
        public DeclarationData()
        {
        }

        public DeclarationData(long jobId, ControlFlowDeclaration declaration)
        {
            Id = $"{jobId}.declaration";
            JobId = jobId.ToString();
            Declaration = declaration;
        }

        public string Id { get; set; } = string.Empty;

        public string JobId { get; set; } = string.Empty;

        public ControlFlowDeclaration? Declaration { get; set; }
    }
}