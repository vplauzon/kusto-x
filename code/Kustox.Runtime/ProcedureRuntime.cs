using Kusto.Cloud.Platform.Data;
using Kusto.Data.Common;
using Kusto.Language;
using Kusto.Language.Syntax;
using Kustox.Compiler;
using Kustox.Runtime.Commands;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using Kustox.Runtime.State.RunStep;
using System.Collections;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Kustox.Runtime
{
    public class ProcedureRuntime
    {
        private readonly KustoxCompiler _compiler;
        private readonly IProcedureRunStore _procedureRunStore;
        private readonly IProcedureRunStepStore _procedureRunStepStore;
        private readonly RunnableRuntime _runnableRuntime;

        public ProcedureRuntime(
            KustoxCompiler compiler,
            IProcedureRunStore procedureRunStore,
            IProcedureRunStepStore procedureRunStepStore,
            RunnableRuntime runnableRuntime)
        {
            _compiler = compiler;
            _procedureRunStore = procedureRunStore;
            _procedureRunStepStore = procedureRunStepStore;
            _runnableRuntime = runnableRuntime;
        }

        public string JobId => _procedureRunStepStore.JobId;

        #region Run Infra
        public async Task<RuntimeResult> RunAsync(
            int? maximumNumberOfSteps,
            CancellationToken ct)
        {
            var levelContext = await RuntimeLevelContext.LoadContextAsync(
                _procedureRunStore,
                _procedureRunStepStore,
                maximumNumberOfSteps,
                ct);
            var declaration = _compiler.CompileProcedure(
                levelContext.LatestProcedureRunStep!.Script);

            if (declaration == null)
            {
                throw new InvalidDataException(
                    $"No declaration for job ID '{_procedureRunStepStore.JobId}'");
            }

            try
            {
                var result = await RunSequenceAsync(
                    declaration,
                    levelContext,
                    ImmutableDictionary<string, TableResult?>.Empty,
                    ct);

                await _procedureRunStore.AppendRunAsync(
                    new[]
                    {
                        new ProcedureRun(
                            _procedureRunStepStore.JobId,
                            ProcedureRunState.Completed,
                            DateTime.UtcNow),
                    },
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
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            if (block.Query != null || block.Command != null)
            {
                levelContext.PreStepExecution();

                var result = await _runnableRuntime.RunStatementAsync(
                    new StatementDeclaration
                    {
                        Command = block.Command,
                        Query = block.Query
                    },
                    captures,
                    ct);

                return block.Capture?.IsScalarCapture == true
                    ? result.ToScalar()
                    : result;
            }
            else if (block.ForEach != null)
            {
                return await RunForEachAsync(
                    block.ForEach,
                    block.Capture?.IsScalarCapture ?? false,
                    stepIndex,
                    levelContext,
                    captures,
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
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            var blocks = sequence.Blocks;
            TableResult? result = null;

            for (int i = 0; i != blocks.Count(); ++i)
            {
                var block = blocks[i];
                var subLevelContext = levelContext.GoDownOneLevel(i, block.Code);
                var captureName = block.Capture?.CaptureName;

                result = subLevelContext.LatestProcedureRunStep?.Result;
                if (subLevelContext.LatestProcedureRunStep?.State != StepState.Completed)
                {
                    await subLevelContext.PersistRunningStepAsync(ct);
                    result = await RunBlockAsync(i, block, subLevelContext, captures, ct);
                    await subLevelContext.PersistCompleteStepAsync(
                        captureName,
                        result,
                        ct);
                }
                if (captureName != null && result != null)
                {
                    captures = captures.Add(captureName, result);
                }
            }

            return result;
        }
        #endregion

        #region For Each
        private async Task<TableResult> RunForEachAsync(
            ForEachDeclaration forEach,
            bool isScalarCapture,
            int stepIndex,
            RuntimeLevelContext levelContext,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            var enumeratorValues = new Stack<TableResult>(
                GetForeachEnumeratorValues(forEach, levelContext, captures).Reverse());

            if (captures.GetCapturedValueIfExist(forEach.Cursor) != null)
            {
                throw new InvalidOperationException(
                    $"Cursor '{forEach.Cursor}' already used in for-each:  "
                    + $"'{forEach.Code}'");
            }
            if (forEach.Sequence.Blocks.Any())
            {
                var subLevelContext = levelContext.GoDownOneLevel(
                    stepIndex,
                    forEach.Code);
                var results = new List<TableResult>(enumeratorValues.Count);
                var subStepIndex = 0;
                var tasks = ImmutableArray<Task<TableResult?>>.Empty;

                await subLevelContext.PersistRunningStepAsync(ct);
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
                        results.AddRange(completedTasks
                            .Where(t => t.Result != null)
                            .Select(t => t.Result!));
                        while (onGoingTasks.Count() < forEach.Concurrency
                            && enumeratorValues.Any())
                        {
                            var iterationValue = enumeratorValues.Pop();

                            if (subLevelContext.LatestProcedureRunStep?.State
                                != StepState.Completed)
                            {
                                onGoingTasks = onGoingTasks.Add(RunForEachIterationAsync(
                                    forEach,
                                    subStepIndex,
                                    subLevelContext,
                                    captures.Add(forEach.Cursor, iterationValue),
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

                    return TableResult.Union(results);
                }
                catch
                {
                    await Task.WhenAll(tasks);

                    throw;
                }
            }
            else
            {   //  Return empty table
                return TableResult.CreateEmpty("Description", "Empty for-loop");
            }
        }

        private async Task<TableResult?> RunForEachIterationAsync(
            ForEachDeclaration forEach,
            int stepIndex,
            RuntimeLevelContext levelContext,
            IImmutableDictionary<string, TableResult?> captures,
            CancellationToken ct)
        {
            var subLevelContext = levelContext.GoDownOneLevel(
                stepIndex,
                forEach.Sequence.Code);

            if (subLevelContext.LatestProcedureRunStep?.State != StepState.Completed)
            {
                await subLevelContext.PersistRunningStepAsync(ct);

                var result = await RunSequenceAsync(
                    forEach.Sequence,
                    subLevelContext,
                    captures,
                    ct);

                await subLevelContext.PersistCompleteStepAsync(
                    null,
                    result!,
                    ct);

                return result;
            }
            else
            {
                return subLevelContext.LatestProcedureRunStep?.Result;
            }
        }

        private static IEnumerable<TableResult> GetForeachEnumeratorValues(
            ForEachDeclaration forEach,
            RuntimeLevelContext levelContext,
            IImmutableDictionary<string, TableResult?> captures)
        {
            var enumatorValue = captures.GetCapturedValueIfExist(forEach.Enumerator)
                ?.AlignDataWithNativeTypes();

            if (enumatorValue == null)
            {
                throw new InvalidOperationException(
                    $"Cannot find enumerator '{forEach.Enumerator}' in for-each:  "
                    + $"'{forEach.Code}'");
            }
            else
            {
                if (enumatorValue.IsScalar)
                {
                    var array = enumatorValue.Data[0][0] as IEnumerable;

                    if (array == null)
                    {
                        throw new InvalidOperationException(
                            $"Scalar enumerator '{forEach.Enumerator}' isn't an array in for-each:  "
                            + $"'{forEach.Code}'");
                    }

                    return array
                        .Cast<object>()
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
        }
        #endregion
    }
}