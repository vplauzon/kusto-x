using Kusto.Data.Common;
using Kusto.Language.Syntax;
using Kusto.Language;
using Kustox.Compiler;
using Kustox.Runtime.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kusto.Cloud.Platform.Data;
using System.Text.Json;
using Kustox.Runtime.State.RunStep;
using Kustox.Runtime.State;

namespace Kustox.Runtime
{
    public class RunnableRuntime
    {
        private readonly ConnectionProvider _connectionProvider;
        private readonly CommandRunnerRouter _commandRunnerRouter;

        public RunnableRuntime(
            ConnectionProvider connectionProvider,
            IStorageHub storageHub,
            IProcedureQueue procedureQueue)
        {
            _connectionProvider = connectionProvider;
            _commandRunnerRouter = new CommandRunnerRouter(
                connectionProvider,
                storageHub,
                procedureQueue);
        }

        public async Task<TableResult> RunStatementAsync(
            StatementDeclaration statementDeclaration,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            if (statementDeclaration.Query != null)
            {
                return await RunQueryAsync(
                    statementDeclaration.Query.Code,
                    captures,
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
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            var queryBlock = (QueryBlock)KustoCode.Parse(query).Syntax;
            var nameReferences = queryBlock
                .GetDescendants<NameReference>()
                .Select(n => n.Name.SimpleName)
                .ToImmutableArray();
            var capturedValues = nameReferences
                .Select(n => KeyValuePair.Create(n, captures.GetCapturedValueIfExist(n)))
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
            var builder = new StringBuilder();

            foreach (var value in capturedValues)
            {
                var name = value.Key;
                var result = value.Value;

                builder.AppendLine($"let {name} = {result.ToKustoExpression()}");
            }

            return builder.ToString();
        }
        #endregion
    }
}