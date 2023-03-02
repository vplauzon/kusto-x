using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Kustox.Runtime.State
{
    public class TableResult
    {
        private static readonly Newtonsoft.Json.JsonSerializer _newtonsoftSerializer =
            Newtonsoft.Json.JsonSerializer.CreateDefault();

        public TableResult(
            bool isScalar,
            IImmutableList<ColumnSpecification> columns,
            IImmutableList<IImmutableList<object>> data)
        {
            IsScalar = isScalar;
            Columns = columns;
            Data = data;
        }

        public TableResult(bool isScalar, DataTable table)
        {
            IsScalar = isScalar;
            Columns = table.Columns
                .Cast<DataColumn>()
                .Select(c => new ColumnSpecification(c.ColumnName, c.DataType.FullName!))
                .ToImmutableArray();
            Data = table.Rows
                .Cast<DataRow>()
                .Select(r => AlignTypesToJsonFriendly(r.ItemArray!))
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();
        }

        public bool IsScalar { get; }

        public IImmutableList<ColumnSpecification> Columns { get; }

        public IImmutableList<IImmutableList<object>> Data { get; }

        public DataTable? ToDataTable()
        {
            var table = new DataTable();

            foreach (var column in Columns)
            {
                table.Columns.Add(new DataColumn(column.ColumnName, column.ColumnType));
            }
            foreach (var dataRow in Data)
            {
                table.Rows.Add(dataRow.ToArray());
            }

            return table;
        }

        public string GetJsonData()
        {
            return JsonSerializer.Serialize(Data);
        }

        private static IImmutableList<object> AlignTypesToJsonFriendly(object[] itemArray)
        {
            var aligned = itemArray
                .Select(o => AlignTypeToJsonFriendly(o))
                .ToImmutableArray();

            return aligned;
        }

        private static object AlignTypeToJsonFriendly(object obj)
        {
            if (obj is sbyte)
            {
                return Convert.ToBoolean(obj);
            }
            else if (obj is SqlDecimal)
            {
                var objDec = (SqlDecimal)obj;

                return objDec.ToSqlMoney().ToDecimal();
            }
            else if (obj is Newtonsoft.Json.Linq.JObject)
            {
                var textWriter = new StringWriter();
                
                _newtonsoftSerializer.Serialize(textWriter, obj);
                
                var text = textWriter.ToString();
                var textObj = JsonSerializer.Deserialize<JsonDocument>(text);

                return textObj!;
            }
            else if (obj is Newtonsoft.Json.Linq.JArray)
            {
                var textWriter = new StringWriter();

                _newtonsoftSerializer.Serialize(textWriter, obj);

                var text = textWriter.ToString();
                var textObj = JsonSerializer.Deserialize<JsonArray>(text);

                return textObj!;
            }
            else
            {
                return obj;
            }
        }
    }
}