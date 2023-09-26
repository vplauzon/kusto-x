using Azure.Core;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
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
            StreamingIngestClient = KustoIngestFactory.CreateStreamingIngestClient(kustoBuilder);
        }

        public TokenCredential Credential { get; }

        public ClientRequestProperties EmptyClientRequestProperties { get; }
            = new ClientRequestProperties();

        public ICslQueryProvider QueryProvider { get; }

        public ICslAdminProvider CommandProvider { get; }

        public IKustoIngestClient StreamingIngestClient { get; }
    }
}