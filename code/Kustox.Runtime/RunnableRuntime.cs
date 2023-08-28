using Kusto.Data.Common;
using Kusto.Language.Syntax;
using Kusto.Language;
using Kustox.Compiler;
using Kustox.Runtime.Commands;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kusto.Cloud.Platform.Data;
using System.Text.Json;
using Kustox.Runtime.State.Run;

namespace Kustox.Runtime
{
    public class RunnableRuntime
    {
        private readonly ConnectionProvider _connectionProvider;
        private readonly CommandRunnerRouter _commandRunnerRouter;

        public RunnableRuntime(ConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            _commandRunnerRouter = new CommandRunnerRouter(connectionProvider);
        }

        public async Task<TableResult> RunStatementAsync(
            RunnableDeclarationBase statementDeclaration,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (statementDeclaration.Query != null)
            {
                return await RunQueryAsync(
                    statementDeclaration.Query.Code,
                    levelContext,
                    ct);
            }
            else if (statementDeclaration.Command != null)
            {
                return await _commandRunnerRouter.RunCommandAsync(
                    statementDeclaration.Command,
                    ct);
            }
            else
            {
                throw new NotSupportedException("statement must be either query or command");
            }
        }

        #region Queries
        private async Task<TableResult> RunQueryAsync(
            string query,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var queryBlock = (QueryBlock)KustoCode.Parse(query).Syntax;
            var nameReferences = queryBlock
                .GetDescendants<NameReference>()
                .Select(n => n.Name.SimpleName)
                .ToImmutableArray();
            var capturedValues = nameReferences
                .Select(n => KeyValuePair.Create(n, levelContext.GetCapturedValueIfExist(n)))
                .Where(p => p.Value != null)
                .ToImmutableArray();
            var queryPrefix = BuildQueryPrefix(capturedValues);
            var reader = await _connectionProvider.QueryProvider.ExecuteQueryAsync(
                string.Empty,
                queryPrefix + query,
                new ClientRequestProperties());
            var table = reader.ToDataSet().Tables[0];

            return table.ToTableResult();
        }

        private string BuildQueryPrefix(
            IImmutableList<KeyValuePair<string, TableResult>> capturedValues)
        {
            var letList = new List<string>();

            foreach (var value in capturedValues)
            {
                var name = value.Key;
                var result = value.Value;

                if (result.IsScalar)
                {
                    var scalarValue = result.Data.First().First();
                    var scalarKustoType = result.Columns.First().GetKustoType();
                    var dynamicValue = $"dynamic({JsonSerializer.Serialize(scalarValue)})";
                    var letValue = $"let {name} = to{scalarKustoType}({dynamicValue});";

                    letList.Add(letValue);
                }
                else
                {
                    var tmp = "__" + Guid.NewGuid().ToString("N");
                    var projections = result.Columns
                        .Zip(Enumerable.Range(0, result.Columns.Count()))
                        .Select(b => new
                        {
                            Name = b.First.ColumnName,
                            KustoType = b.First.GetKustoType(),
                            Index = b.Second
                        })
                        .Select(b => $"{b.Name}=to{b.KustoType}({tmp}[{b.Index}])");

                    letList.Add(@$"let {name} = print {tmp} = dynamic({result.GetJsonData()})
| mv-expand {tmp}
| project {string.Join(", ", projections)};");
                }
            }
            var prefixText = string.Join(Environment.NewLine, letList) + Environment.NewLine;

            return prefixText;
        }
        #endregion
    }
}