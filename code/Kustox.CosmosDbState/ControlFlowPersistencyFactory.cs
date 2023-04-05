using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.CosmosDbState
{
    public static class ControlFlowPersistencyFactory
    {
        public static IProcedureRunList FromAccessKeys(
            string accountEndpoint,
            string accessKey,
            string databaseName,
            string containerName)
        {
            var client =
                new CosmosClient(accountEndpoint, accessKey, CreateCosmosClientOptions());
            var container = client.GetContainer(databaseName, containerName);

            return new CosmosDbProcedureRunList(container);
        }

        public static IProcedureRunList FromEnvironmentVariables()
        {
            var accountEndpoint = GetEnvironmentVariable("cosmosDbAccountEndpoint");
            var accessKey = GetEnvironmentVariable("cosmosDbAccessKey");
            var databaseName = GetEnvironmentVariable("cosmosDbDatabase");
            var containerName = GetEnvironmentVariable("cosmosDbContainer");

            return FromAccessKeys(accountEndpoint, accessKey, databaseName, containerName);
        }

        private static string GetEnvironmentVariable(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName);

            if (value == null)
            {
                throw new ArgumentNullException(variableName, "Environment variable");
            }
            else
            {
                return value;
            }
        }

        private static CosmosClientOptions CreateCosmosClientOptions()
        {
            return new CosmosClientOptions
            {
                ConsistencyLevel = ConsistencyLevel.Strong,
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                    IgnoreNullValues = true
                }
            };
        }
    }
}