﻿using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
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

namespace Kustox.Runtime
{
    internal static class DataTableHelper
    {
        private static readonly Newtonsoft.Json.JsonSerializer _newtonsoftSerializer =
            Newtonsoft.Json.JsonSerializer.CreateDefault();

        public static TableResult ToTableResult(this DataTable table, bool isScalar = false)
        {
            var tableData = table.Rows
                .Cast<DataRow>()
                .Select(r => r.ItemArray.Select(o => AlignTypeToJsonFriendly(o!)))
                .Select(r => r.ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();
            var columns = table.Columns
                  .Cast<DataColumn>()
                  .Select(c => new ColumnSpecification(c.ColumnName, c.DataType.FullName!))
                  .ToImmutableArray();
            var result = new TableResult(isScalar, columns, tableData);

            return result;
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
                var textObj = JsonSerializer.Deserialize<JsonValue>(text);

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