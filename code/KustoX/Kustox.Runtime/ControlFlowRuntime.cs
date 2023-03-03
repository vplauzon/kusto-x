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
            BlockDeclaration instuction,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (instuction.Capturable != null)
            {
                return await RunCapturableAsync(
                    instuction.Capturable,
                    stepIndex,
                    levelContext,
                    ct);
            }
            else
            {
                throw new NotSupportedException("Block declaration");
            }
        }

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
                }
            }

            return result;
        }

        private async Task<TableResult> RunCapturableAsync(
            CaptureDeclaration declaration,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            levelContext.PreStepExecution();

            if (declaration.Runnable.Query != null)
            {
                var reader = await ExecuteQueryAsync(declaration, levelContext, ct);

                return await CaptureResultAsync(declaration, stepIndex, reader, levelContext, ct);
            }
            else if (declaration.Runnable.Command != null)
            {
                var reader = await _commandProvider.ExecuteControlCommandAsync(
                    string.Empty,
                    declaration.Runnable.Command,
                    new ClientRequestProperties());

                return await CaptureResultAsync(declaration, stepIndex, reader, levelContext, ct);
            }
            else
            {
                throw new NotSupportedException("runnable must be either query or command");
            }
        }

        private async Task<IDataReader> ExecuteQueryAsync(
            CaptureDeclaration declaration,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var queryBlock = (QueryBlock)KustoCode.Parse(declaration.Runnable.Query).Syntax;
            var nameReferences = queryBlock
                .GetDescendants<NameReference>()
                .Select(n => n.Name.SimpleName)
                .ToImmutableArray();
            var capturedValues = levelContext.GetCapturedValues(nameReferences);
            var queryPrefix = BuildQueryPrefix(capturedValues);
            var reader = await _queryProvider.ExecuteQueryAsync(
                string.Empty,
                queryPrefix + declaration.Runnable.Query,
                new ClientRequestProperties());

            return reader;
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

        private async Task<TableResult> CaptureResultAsync(
            CaptureDeclaration declaration,
            int stepIndex,
            IDataReader reader,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var table = reader.ToDataSet().Tables[0];
            var result = new TableResult(declaration.IsScalarCapture ?? false, table);

            await levelContext.CompleteStepAsync(
                stepIndex,
                declaration.Code,
                declaration.CaptureName,
                result,
                ct);

            return result;
        }
    }
}