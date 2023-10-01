using Kusto.Language.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Kustox.Runtime.State.RunStep
{
    public class TableResult
    {
        public TableResult(
            bool isScalar,
            IImmutableList<ColumnSpecification> columns,
            IImmutableList<IImmutableList<object>> data)
        {
            IsScalar = isScalar;
            if (isScalar)
            {
                if (columns.Count != 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(columns),
                        $"For scalar, must be a single column but there are {columns.Count()}");
                }
                if (data.Count != 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(data),
                        $"For scalar, must be a single row but there are {data.Count()}");
                }
                if (data.First().Count != 1)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(data),
                        $"For scalar, must be a single column in the single row"
                        + $" but there are {data.First().Count()}");
                }
            }
            if (columns.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), "There are no columns!");
            }
            if (data.Select(row => row.Count()).Any(l => l != columns.Count()))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(data),
                    "Some row(s) don't have the right column count");
            }
            Columns = columns;
            Data = data;
        }

        public static TableResult CreateEmpty(string columnName, string oneCellContent)
        {
            return new TableResult(
                false,
                ImmutableArray<ColumnSpecification>.Empty.Add(
                    new ColumnSpecification(columnName, typeof(string))),
                ImmutableArray<IImmutableList<object>>.Empty.Add(
                    ImmutableArray<object>.Empty.Add(oneCellContent)));
        }

        public bool IsScalar { get; }

        public IImmutableList<ColumnSpecification> Columns { get; }

        public IImmutableList<IImmutableList<object>> Data { get; }

        public TableResult ToScalar()
        {
            return new TableResult(
                true,
                Columns.Count == 1 ? Columns : Columns.Take(1).ToImmutableArray(),
                Data.Count == 1 && Data.First().Count == 1
                ? Data
                : Data
                .Take(1)
                .Select(r => r.Take(1).ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray());
        }

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

        public string ToKustoExpression()
        {
            if (IsScalar)
            {
                var scalarValue = Data.First().First();
                var scalarKustoType = Columns.First().GetKustoType();
                var dynamicValue = $"dynamic({JsonSerializer.Serialize(scalarValue)})";
                var finalValue = $"to{scalarKustoType}({dynamicValue});";

                return finalValue;
            }
            else
            {
                var tmp = "__" + Guid.NewGuid().ToString("N");
                var tableValue = @$"
print {tmp} = dynamic({GetJsonData()})
| mv-expand {tmp}
| project {ToDynamicProjection(tmp)};";

                return tableValue;
            }
        }

        public string ToDynamicProjection(string dynamicColumnName)
        {
            var projections = Columns
                .Zip(Enumerable.Range(0, Columns.Count()))
                .Select(b => new
                {
                    Name = b.First.ColumnName,
                    KustoType = b.First.GetKustoType(),
                    Index = b.Second
                })
                .Select(b => $"{b.Name}=to{b.KustoType}({dynamicColumnName}[{b.Index}])");
            var kql = string.Join(", ", projections);

            return kql;
        }

        public string GetJsonData()
        {
            return JsonSerializer.Serialize(Data);
        }

        public IEnumerable<object> GetColumnData(int columnIndex)
        {
            foreach (var array in Data)
            {
                yield return array[columnIndex];
            }
        }

        public IEnumerable<object> GetColumnData(string columnName)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].ColumnName == columnName)
                {
                    return GetColumnData(i);
                }
            }

            throw new ArgumentOutOfRangeException(nameof(columnName), "No such column name");
        }

        public TableResult AlignDataWithNativeTypes()
        {
            var alignedData = Data
                .Select(row => row.Select(item => AlignWithNativeTypes(item)).ToImmutableArray())
                .Cast<IImmutableList<object>>()
                .ToImmutableArray();

            return new TableResult(IsScalar, Columns, alignedData);
        }

        public static TableResult Union(IEnumerable<TableResult> results)
        {
            if (!results.Any())
            {
                throw new ArgumentOutOfRangeException(
                    nameof(results),
                    $"Union requires at list one {typeof(TableResult).Name}");
            }
            if (results.Any(r => r == null))
            {
                throw new ArgumentNullException(
                    nameof(results),
                    $"Some or all {typeof(TableResult).Name} are null");
            }

            var schemas = results.Select(r => r.Columns);
            var template = schemas.First();

            foreach (var schema in schemas)
            {
                if (!Enumerable.SequenceEqual(schema, template))
                {
                    throw new InvalidDataException(
                        "Not all results have the same schema for union");
                }
            }

            var datas = new Stack<IEnumerable<IImmutableList<object>>>(
                results.Select(r => r.Data).Reverse());

            while (datas.Count() > 1)
            {
                var first = datas.Pop();
                var second = datas.Pop();
                var union = first.Concat(second);

                datas.Push(union);
            }

            var allUnion = datas.Pop().ToImmutableArray();

            return new TableResult(false, template, allUnion);
        }

        private static object AlignWithNativeTypes(object item)
        {
            if (item is IEnumerable enumerable)
            {
                var newArray = enumerable
                    .Cast<object>()
                    .Select(subItem => AlignWithNativeTypes(subItem))
                    .ToImmutableArray();

                return newArray;
            }
            else if (item is JsonElement element)
            {
                switch (element.ValueKind)
                {
                    case JsonValueKind.Array:
                        return AlignWithNativeTypes(element.EnumerateArray().ToImmutableArray());
                    case JsonValueKind.String:
                        return element.GetString()!;
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.True:
                        return true;
                    default:
                        return item;
                }
            }
            else
            {
                return item;
            }
        }
    }
}