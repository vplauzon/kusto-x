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
            IImmutableList<ColumnSpecification> columns,
            IImmutableList<IImmutableList<object?>> data)
        {
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
            IsScalar = false;
            Columns = columns;
            Data = data;
        }

        public TableResult(Type scalarType, object? scalarData)
        {
            IsScalar = true;
            Columns = ImmutableArray<ColumnSpecification>.Empty.Add(
                new ColumnSpecification("c", scalarType));
            Data = ImmutableArray<IImmutableList<object?>>.Empty.Add(
                    ImmutableArray<object?>.Empty.Add(scalarData));
        }

        public static TableResult CreateEmpty(string columnName, string oneCellContent)
        {
            return new TableResult(
                ImmutableArray<ColumnSpecification>.Empty.Add(
                    new ColumnSpecification(columnName, typeof(string))),
                ImmutableArray<IImmutableList<object>>.Empty.Add(
                    ImmutableArray<object>.Empty.Add(oneCellContent)));
        }

        public bool IsScalar { get; }

        public IImmutableList<ColumnSpecification> Columns { get; }

        public IImmutableList<IImmutableList<object?>> Data { get; }

        public TableResult ToScalar()
        {
            if (Data.Count > 0 && Data.First().Count > 0)
            {
                return new TableResult(Columns[0].ColumnType, Data[0][0]);
            }
            else
            {
                return new TableResult(typeof(object), null);
            }
        }

        public string ToKustoExpression()
        {
            if (IsScalar)
            {
                var scalarValue = Data.First().First();
                var scalarKustoType = Columns.First().GetKustoType();
                var dynamicValue = $"dynamic({JsonSerializer.Serialize(scalarValue)})";
                var finalValue = $"to{scalarKustoType}({dynamicValue})";

                return finalValue;
            }
            else
            {
                var columnDeclaration = Columns
                    .Select(c => $"['{c.ColumnName}']:{c.GetKustoType()}");
                Func<(ColumnSpecification, object?), string> renderData = p =>
                {
                    var kustoType = p.Item1.GetKustoType();
                    var jsonValue = JsonSerializer.Serialize(p.Item2);

                    if (kustoType == "string")
                    {
                        return jsonValue;
                    }
                    else
                    {
                        return $"{kustoType}({jsonValue})";
                    }
                };
                var data = Data
                    .Select(row => row.Zip(Columns, (d, c) => (c, d)).Select(p => renderData(p)))
                    .SelectMany(r => r);
                var expression = @$"
datatable({string.Join(", ", columnDeclaration)}) [
{string.Join(", " + Environment.NewLine, data)}
]";

                return expression;
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

        public IEnumerable<object?> GetColumnData(int columnIndex)
        {
            foreach (var array in Data)
            {
                yield return array[columnIndex];
            }
        }

        public IEnumerable<object?> GetColumnData(string columnName)
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

            return IsScalar
                ? new TableResult(Columns[0].ColumnType, alignedData[0][0])
                : new TableResult(Columns, alignedData);
        }

        public static TableResult Union(IEnumerable<TableResult> results)
        {
            if (!results.Any())
            {
                throw new ArgumentOutOfRangeException(
                    nameof(results),
                    $"Union requires at list one {typeof(TableResult).Name}");
            }
            if (results.Any(r => r.IsScalar))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(results),
                    $"Union requires non scalar {typeof(TableResult).Name}");
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

            var datas = new Stack<IEnumerable<IImmutableList<object?>>>(
                results.Select(r => r.Data).Reverse());

            while (datas.Count() > 1)
            {
                var first = datas.Pop();
                var second = datas.Pop();
                var union = first.Concat(second);

                datas.Push(union);
            }

            var allUnion = datas.Pop().ToImmutableArray();

            return new TableResult(template, allUnion);
        }

        private static object? AlignWithNativeTypes(object? item)
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