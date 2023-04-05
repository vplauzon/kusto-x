using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.CosmosDbState.DataObjects
{
    internal static class ProcedureRunDataHelper
    {
        public static PartitionKey JobIdToPartitionKey(long jobId)
        {
            return new PartitionKey(jobId.ToString());
        }
    }
}