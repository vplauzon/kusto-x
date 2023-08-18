using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Text.Json;
using System.Text.Json.Nodes;
using static System.Net.Mime.MediaTypeNames;

namespace Kustox.BlobStorageState.DataObjects
{
    public class TableData
    {
        private static readonly Newtonsoft.Json.JsonSerializer _newtonsoftSerializer =
            Newtonsoft.Json.JsonSerializer.CreateDefault();

        public TableData()
        {
        }

        public TableData(TableResult result)
        {
            IsScalar = result.IsScalar;
            ColumnNames = result.Columns
                .Select(c => c.ColumnName)
                .ToImmutableArray();
            ColumnTypes = result.Columns
                .Select(c => c.ColumnType.FullName)
                .ToImmutableArray();
            Data = result.Data
                .Select(r => r.Select(o => AlignTypeToNewtonsoftFriendly(o)).ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();
        }

        public bool IsScalar { get; set; }

        public IImmutableList<string>? ColumnNames { get; set; }

        public IImmutableList<string>? ColumnTypes { get; set; }

        public IImmutableList<IImmutableList<object>>? Data { get; set; }

        private static object AlignTypeToNewtonsoftFriendly(object obj)
        {
            if (obj is JsonValue)
            {
                var text = JsonSerializer.Serialize(obj);
                var textReader = new StringReader(text);
                var jsonTextReader = new Newtonsoft.Json.JsonTextReader(textReader);
                var textObj = _newtonsoftSerializer.Deserialize<Newtonsoft.Json.Linq.JObject>(
                    jsonTextReader);

                return textObj!;
            }
            else if (obj is JsonArray)
            {
                var text = JsonSerializer.Serialize(obj);
                var textReader = new StringReader(text);
                var jsonTextReader = new Newtonsoft.Json.JsonTextReader(textReader);
                var textObj = _newtonsoftSerializer.Deserialize<Newtonsoft.Json.Linq.JArray>(
                    jsonTextReader);

                return textObj!;
            }
            else
            {
                return obj;
            }
        }
    }
}