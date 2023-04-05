﻿using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kusto.Language;
using Kusto.Language.Syntax;
using Kustox.Compiler;
using Kustox.Runtime.Commands;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Kustox.Runtime
{
    public class ProcedureRuntime
    {
        private readonly IProcedureRun _controlFlowInstance;
        private readonly ConnectionProvider _connectionProvider;
        private readonly CommandRunnerRouter _commandRunnerRouter;

        public ProcedureRuntime(
            IProcedureRun controlFlowInstance,
            ConnectionProvider connectionProvider)
        {
            _controlFlowInstance = controlFlowInstance;
            _connectionProvider = connectionProvider;
            _commandRunnerRouter = new CommandRunnerRouter(connectionProvider);
        }

        #region Run Infra
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
                    ProcedureRunState.Completed,
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
                levelContext.PreStepExecution();

                return await RunQueryAsync(
                    block.Query,
                    block.Capture?.IsScalarCapture ?? false,
                    stepIndex,
                    levelContext,
                    ct);
            }
            else if (block.Command != null)
            {
                levelContext.PreStepExecution();

                return await _commandRunnerRouter.RunCommandAsync(
                    block,
                    block.Capture?.IsScalarCapture ?? false,
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
        #endregion

        #region Sequences
        private async Task<TableResult?> RunSequenceAsync(
            SequenceDeclaration sequence,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var stepStates = levelContext.GetLevelStepStates();
            var blocks = sequence.Blocks;
            TableResult? result = null;

            for (int i = 0; i != blocks.Count(); ++i)
            {
                var block = blocks[i];

                if (stepStates.Count() <= i || stepStates[i] != StepState.Completed)
                {
                    await levelContext.PersistRunningStepAsync(i, block.Code, ct);
                    result = await RunBlockAsync(i, block, levelContext, ct);
                    await levelContext.PersistCompleteStepAsync(
                        i,
                        block.Code,
                        block.Capture?.CaptureName,
                        result,
                        ct);
                }
            }
            if (result == null && blocks.Any())
            {   //  No result in-memory but one should exist in persistency
                var allSteps = await levelContext.GetAllStepsAsync(ct);
                var lastStep = allSteps.Last();

                result = lastStep.Result;
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

        #region For Each
        private async Task<TableResult> RunForEachAsync(
            ForEachDeclaration forEach,
            bool isScalarCapture,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var enumeratorValues = new Stack<TableResult>(
                GetForeachEnumeratorValues(forEach, levelContext).Reverse());

            if (levelContext.GetCapturedValueIfExist(forEach.Cursor) != null)
            {
                throw new InvalidOperationException(
                    $"Cursor '{forEach.Cursor}' already used in for-each:  "
                    + $"'{forEach.Code}'");
            }
            if (forEach.Sequence.Blocks.Any())
            {
                var subLevelContext = await levelContext.GoDownOneLevelAsync(stepIndex, ct);
                var subLevelStates = subLevelContext.GetLevelStepStates();
                var subStepIndex = 0;
                var tasks = ImmutableArray<Task>.Empty;

                try
                {
                    while (enumeratorValues.Any() || tasks.Any())
                    {
                        var isCompleted = tasks.Select(t => t.IsCompleted).ToImmutableArray();
                        var completedTasks = tasks
                            .Zip(isCompleted)
                            .Where(c => c.Second)
                            .Select(c => c.First)
                            .ToImmutableArray();
                        var onGoingTasks = tasks
                            .Zip(isCompleted)
                            .Where(c => !c.Second)
                            .Select(c => c.First)
                            .ToImmutableArray();

                        await Task.WhenAll(completedTasks);
                        while (onGoingTasks.Count() < forEach.Concurrency
                            && enumeratorValues.Any())
                        {
                            var iterationValue = enumeratorValues.Pop();

                            if (subLevelStates.Count() <= subStepIndex
                                || subLevelStates[subStepIndex] != StepState.Completed)
                            {
                                onGoingTasks = onGoingTasks.Add(RunForEachIterationAsync(
                                    forEach,
                                    iterationValue,
                                    subStepIndex,
                                    subLevelContext,
                                    ct));
                            }
                            ++subStepIndex;
                        }
                        if (onGoingTasks.Any())
                        {
                            await Task.WhenAny(onGoingTasks);
                        }
                        tasks = onGoingTasks;
                    }
                }
                catch
                {
                    await Task.WhenAll(tasks);
                }

                return await FetchForEachResultAsync(subLevelContext, ct);
            }
            else
            {   //  Return empty table
                return new TableResult(
                    false,
                    ImmutableArray<ColumnSpecification>.Empty.Add(
                        new ColumnSpecification("Description", typeof(string))),
                    ImmutableArray<IImmutableList<object>>.Empty.Add(
                        ImmutableArray<object>.Empty.Add("Empty for-loop")));
            }
        }

        private async Task<TableResult> FetchForEachResultAsync(
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var states = await levelContext.GetAllStepsAsync(ct);
            var nonCompletedState = states.FirstOrDefault(s => s.State != StepState.Completed);

            if (nonCompletedState != null)
            {
                throw new InvalidOperationException(
                    $"One of for-each sub step has state '{nonCompletedState}'");
            }

            var results = states
                .Select(s => s.Result!)
                .ToImmutableArray();

            return TableResult.Union(results);
        }

        private async Task RunForEachIterationAsync(
            ForEachDeclaration forEach,
            TableResult item,
            int stepIndex,
            RuntimeLevelContext levelContext,
            CancellationToken ct)
        {
            var subLevelContext = await levelContext.GoDownOneLevelAsync(stepIndex, ct);

            subLevelContext.AddCapturedValue(forEach.Cursor, item);
            await levelContext.PersistRunningStepAsync(stepIndex, forEach.Sequence.Code, ct);
            var result = await RunSequenceAsync(forEach.Sequence, subLevelContext, ct);
            await levelContext.PersistCompleteStepAsync(
                stepIndex,
                forEach.Sequence.Code,
                null,
                result!,
                ct);
        }

        private static IEnumerable<TableResult> GetForeachEnumeratorValues(
            ForEachDeclaration forEach,
            RuntimeLevelContext levelContext)
        {
            var enumatorValue = levelContext.GetCapturedValueIfExist(forEach.Enumerator);

            if (enumatorValue == null)
            {
                throw new InvalidOperationException(
                    $"Cannot find enumerator '{forEach.Enumerator}' in for-each:  "
                    + $"'{forEach.Code}'");
            }
            if (enumatorValue.IsScalar)
            {
                var array = enumatorValue.Data[0][0] as IEnumerable<object>;

                if (array == null)
                {
                    throw new InvalidOperationException(
                        $"Scalar enumerator '{forEach.Enumerator}' isn't an array in for-each:  "
                        + $"'{forEach.Code}'");
                }

                return array
                    .Select(o => new TableResult(
                        true,
                        ImmutableArray.Create(new ColumnSpecification("c", typeof(object))),
                        ImmutableArray.Create(ImmutableArray.Create(o) as IImmutableList<object>)));
            }
            else
            {
                return enumatorValue
                    .GetColumnData(0)
                    .Select(o => new TableResult(
                        true,
                        ImmutableArray.Create(enumatorValue.Columns.First()),
                        ImmutableArray.Create(ImmutableArray.Create(o) as IImmutableList<object>)));
            }
        }
        #endregion
    }
}