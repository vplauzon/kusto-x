using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kusto.Language;
using Kusto.Language.Syntax;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Kustox.Runtime
{
    public class ControlFlowRuntime
    {
        private readonly IControlFlowInstance _controlFlowInstance;
        private readonly ICslQueryProvider _queryProvider;
        private readonly ICslAdminProvider _commandProvider;

        public ControlFlowRuntime(
            IControlFlowInstance controlFlowInstance,
            ICslQueryProvider queryProvider,
            ICslAdminProvider commandProvider)
        {
            _controlFlowInstance = controlFlowInstance;
            _queryProvider = queryProvider;
            _commandProvider = commandProvider;
        }

        public async Task<RuntimeResult> RunAsync(
            int? maximumNumberOfSteps = null,
            CancellationToken ct = default(CancellationToken))
        {
            var levelContext = await RuntimeLevelContext.LoadContextAsync(
                _controlFlowInstance,
                maximumNumberOfSteps,
                ct);

            try
            {
                var result = await RunSequenceAsync(
                    levelContext.Declaration.RootSequence,
                    levelContext,
                    ct);

                await _controlFlowInstance.SetControlFlowStateAsync(
                    ControlFlowState.Completed,
                    ct);

                return new RuntimeResult(true, result);
            }
            catch (TaskCanceledException)
            {   //  Do nothing, it will need to run again
                return new RuntimeResult(false, null);
            }
        }

        private async Task<TableResult> RunBlockAsync(
            int stepIndex,
            BlockDeclaration block,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (block.Query != null)
            {
                return await RunQueryAsync(
                    block.Query,
                    block.Capture?.IsScalarCapture ?? false,
                    stepIndex,
                    levelContext,
                    ct);
            }
            else if (block.Command != null)
            {
                return await RunCommandAsync(
                    block.Command,
                    block.Capture?.IsScalarCapture ?? false,
                    stepIndex,
                    levelContext,
                    ct);
            }
            else if (block.ForEach != null)
            {
                return await RunForEachAsync(
                    block.ForEach,
                    block.Capture?.IsScalarCapture ?? false,
                    stepIndex,
                    levelContext,
                    ct);
            }
            else
            {
                throw new NotSupportedException("runnable must be either query or command");
            }
        }

        #region Sequences
        private async Task<TableResult?> RunSequenceAsync(
            SequenceDeclaration sequence,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var steps = await levelContext.RestoreStepsAsync(ct);
            var blocks = sequence.Blocks;
            TableResult? result = null;

            for (int i = 0; i != blocks.Count(); ++i)
            {
                var block = blocks[i];

                if (steps.Count() <= i || steps[i].State != StepState.Completed)
                {
                    result = await RunBlockAsync(i, block, levelContext, ct);
                    await levelContext.CompleteStepAsync(
                        i,
                        block.Code,
                        block.Capture?.CaptureName,
                        result,
                        ct);
                }
            }

            return result;
        }
        #endregion

        #region Queries
        private async Task<TableResult> RunQueryAsync(
            string query,
            bool isScalarCapture,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            levelContext.PreStepExecution();

            var queryBlock = (QueryBlock)KustoCode.Parse(query).Syntax;
            var nameReferences = queryBlock
                .GetDescendants<NameReference>()
                .Select(n => n.Name.SimpleName)
                .ToImmutableArray();
            var capturedValues = levelContext.GetCapturedValues(nameReferences);
            var queryPrefix = BuildQueryPrefix(capturedValues);
            var reader = await _queryProvider.ExecuteQueryAsync(
                string.Empty,
                queryPrefix + query,
                new ClientRequestProperties());
            var table = reader.ToDataSet().Tables[0];
            var result = new TableResult(isScalarCapture, table);

            return result;
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

        #region Commands
        private async Task<TableResult> RunCommandAsync(
            string command,
            bool isScalarCapture,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            levelContext.PreStepExecution();

            var reader = await _commandProvider.ExecuteControlCommandAsync(
                string.Empty,
                command,
                new ClientRequestProperties());
            var table = reader.ToDataSet().Tables[0];
            var result = new TableResult(isScalarCapture, table);

            return result;
        }
        #endregion

        #region For Each
        private Task<TableResult> RunForEachAsync(
            ForEachDeclaration forEach,
            bool isScalarCapture,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}