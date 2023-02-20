using Kustox.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.CosmosDbPersistency.DataObjects
{
    internal record JobData(
        string? id,
        string jobId,
        ControlFlowDeclaration? declaration);
}