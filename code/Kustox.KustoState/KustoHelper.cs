using Kusto.Cloud.Platform.Data;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data.Common;
using Kusto.Ingest;
using Kustox.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kustox.KustoState
{
    internal static class KustoHelper
    {
        private readonly static JsonSerializerOptions JSON_SERIALIZER_OPTIONS =
            new JsonSerializerOptions();

        static KustoHelper()
        {
            JSON_SERIALIZER_OPTIONS.Converters.Add(new JsonStringEnumConverter());
        }

        public static byte[] Serialize(object data)
        {
            using (var stream = new MemoryStream())
            {
                JsonSerializer.Serialize(stream, data, JSON_SERIALIZER_OPTIONS);
                //  Append "return"
                stream.WriteByte((byte)'\n');

                return stream.ToArray();
            }
        }

        public static async Task StreamIngestAsync(
            IKustoIngestClient ingestClient,
            string dbName,
            string tableName,
            IEnumerable<object> data,
            CancellationToken ct)
        {
            using (var stream = new MemoryStream())
            {
                foreach (var d in data)
                {
                    JsonSerializer.Serialize(stream, d, JSON_SERIALIZER_OPTIONS);
                    //  Append "return"
                    stream.WriteByte((byte)'\n');
                }

                stream.Position = 0;
                await ingestClient.IngestFromStreamAsync(
                    stream,
                    new KustoIngestionProperties(dbName, tableName)
                    {
                        Format = DataSourceFormat.json
                    });
            }
        }

        public static async Task<IImmutableList<T>> QueryAsync<T>(
            ICslQueryProvider queryProvider,
            string queryText,
            CancellationToken ct)
        {
            var properties = typeof(T)
                .GetProperties()
                .Select(p => p.Name);
            var propertyBag = properties
                .Select(p => $"'{p}', {p}");
            var allPropertyBags = string.Join(", ", propertyBag);
            var jsonQuery = $"{queryText} | project tostring(bag_pack({allPropertyBags}))";

            using (var reader = await queryProvider.ExecuteQueryAsync(
                null,
                jsonQuery,
                new ClientRequestProperties(),
                ct))
            {
                var table = reader.ToDataSet().Tables[0];
                var textItems = table.Rows
                    .Cast<DataRow>()
                    .Select(r => (string?)r.ItemArray[0]);
                var items = textItems
                    .Select(t => JsonSerializer.Deserialize<T>(t!, JSON_SERIALIZER_OPTIONS))
                    .ToImmutableArray();

                return items;
            }
        }
    }
}