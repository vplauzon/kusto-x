using Azure.Core;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    public class ConnectionProvider
    {
        public ConnectionProvider(
            Uri clusterUri,
            string database,
            TokenCredential credential)
        {
            var kustoBuilder = new KustoConnectionStringBuilder(clusterUri.ToString(), database)
                .WithAadAzureTokenCredentialsAuthentication(credential);

            Credential = credential;
            QueryProvider = KustoClientFactory.CreateCslQueryProvider(kustoBuilder);
            CommandProvider = KustoClientFactory.CreateCslAdminProvider(kustoBuilder);
        }

        public TokenCredential Credential { get; }
     
        public ICslQueryProvider QueryProvider { get; }

        public ICslAdminProvider CommandProvider { get; }
    }
}