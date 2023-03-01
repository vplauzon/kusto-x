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

        public async Task<bool> RunAsync(
            int? maximumNumberOfSteps = null,
            CancellationToken ct = default(CancellationToken))
        {
            var levelContext = await RuntimeLevelContext.LoadContextAsync(
                _controlFlowInstance,
                maximumNumberOfSteps,
                ct);

            try
            {
                await RunSequenceAsync(
                    levelContext.Declaration.RootSequence,
                    levelContext,
                    ct);
                await _controlFlowInstance.SetControlFlowStateAsync(
                    ControlFlowState.Completed,
                    ct);

                return true;
            }
            catch (TaskCanceledException)
            {   //  Do nothing, it will need to run again
                return false;
            }
        }

        private async Task RunBlockAsync(
            int stepIndex,
            BlockDeclaration instuction,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (instuction.Capturable != null)
            {
                await RunCapturableAsync(instuction.Capturable, stepIndex, levelContext, ct);
            }
            else
            {
                throw new NotSupportedException("Block declaration");
            }
        }

        private async Task RunSequenceAsync(
            SequenceDeclaration sequence,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var steps = await levelContext.GetStepsAsync(ct);
            var blocks = sequence.Blocks;

            for (int i = 0; i != blocks.Count(); ++i)
            {
                if (steps.Count() <= i || steps[i].State != StepState.Completed)
                {
                    var block = blocks[i];
                    
                    await RunBlockAsync(i, block, levelContext, ct);
                }
            }
        }

        private async Task RunCapturableAsync(
            CaptureDeclaration declaration,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            if (declaration.Runnable.Query != null)
            {
                var reader = await ExecuteQueryAsync(declaration, levelContext, ct);

                await CaptureResultAsync(declaration, stepIndex, reader, levelContext, ct);
            }
            else if (declaration.Runnable.Command != null)
            {
                var reader = await _commandProvider.ExecuteControlCommandAsync(
                    string.Empty,
                    declaration.Runnable.Command,
                    new ClientRequestProperties());

                await CaptureResultAsync(declaration, stepIndex, reader, levelContext, ct);
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
            var parameters = BuildQueryParameters(capturedValues);
            var reader = await _queryProvider.ExecuteQueryAsync(
                string.Empty,
                parameters.queryPrefix + declaration.Runnable.Query,
                parameters.properties);

            return reader;
        }

        private (ClientRequestProperties properties, string queryPrefix) BuildQueryParameters(
            IImmutableList<KeyValuePair<string, TableResult>> capturedValues)
        {
            var properties = new ClientRequestProperties();
            var letList = new List<string>();
            var declareList = new List<string>();
            var typeMapping = new (Type, string, Action<string, object>)[]
            {
                (typeof(bool), "bool", (name, value)=>properties.SetParameter(name, (bool)value)),
                (typeof(DateTime), "datetime", (name, value)=>properties.SetParameter(name, (DateTime)value)),
                (typeof(double), "double", (name, value)=>properties.SetParameter(name, (double)value)),
                (typeof(Guid), "guid", (name, value)=>properties.SetParameter(name, (Guid)value)),
                (typeof(int), "int", (name, value)=>properties.SetParameter(name, (int)value)),
                (typeof(long), "long", (name, value)=>properties.SetParameter(name, (long)value)),
                (typeof(string), "string", (name, value)=>properties.SetParameter(name, (string)value)),
                (typeof(TimeSpan), "timespan", (name, value)=>properties.SetParameter(name, (TimeSpan)value))
            }.ToImmutableDictionary(
                t => t.Item1,
                t => new { KustoTypeName = t.Item2, ScalarAction = t.Item3 });

            foreach (var value in capturedValues)
            {
                var name = value.Key;
                var result = value.Value;

                if (result.IsScalar)
                {
                    var scalarValue = result.Data.First().First();
                    var scalarType = result.Columns.First().ColumnType;

                    if (typeMapping.ContainsKey(scalarType))
                    {
                        var typeActions = typeMapping[scalarType];

                        if (scalarValue == null)
                        {
                            letList.Add($"let {name} = {typeActions.KustoTypeName}(null);");
                        }
                        else
                        {
                            typeActions.ScalarAction(name, scalarValue);
                            declareList.Add($"{name}:{typeActions.KustoTypeName}");
                        }
                    }
                    else
                    {
                        throw new NotImplementedException($"Scalar type:  {scalarType.FullName}");
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            var declareText = declareList.Any()
                ? $"declare query_parameters({string.Join(", ", declareList)});"
                : string.Empty;
            var prefixText = declareText
                + Environment.NewLine
                + string.Join(Environment.NewLine, letList);

            return (properties, prefixText);
        }

        private async Task CaptureResultAsync(
            CaptureDeclaration declaration,
            int stepIndex,
            IDataReader reader,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var table = reader.ToDataSet().Tables[0];

            await levelContext.CompleteStepAsync(
                stepIndex,
                declaration.CaptureName,
                new TableResult(
                    declaration.IsScalarCapture ?? false,
                    table),
                ct);
        }
    }
}