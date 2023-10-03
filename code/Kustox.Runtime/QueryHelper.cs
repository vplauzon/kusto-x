using Kusto.Language.Syntax;
using Kusto.Language;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    internal static class QueryHelper
    {
        public static string BuildQueryPrefix(
            string query,
            IImmutableDictionary<string, TableResult?> captures)
        {
            var queryBlock = (QueryBlock)KustoCode.Parse(query).Syntax;
            var nameReferences = queryBlock
                .GetDescendants<NameReference>()
                .Select(n => n.Name.SimpleName)
                .ToImmutableArray();
            var capturedValues = nameReferences
                .Select(n => KeyValuePair.Create(n, captures.GetCapturedValueIfExist(n)))
                .Where(p => p.Value != null)
                .Select(p=> KeyValuePair.Create(p.Key, p.Value!))
                .ToImmutableArray();
            var builder = new StringBuilder();

            foreach (var value in capturedValues)
            {
                var name = value.Key;
                var result = value.Value;

                builder.AppendLine($"let {name} = {result.ToKustoExpression()}");
            }

            return builder.ToString();
        }
    }
}