using Kusto.Cloud.Platform.Data;
using Kusto.Cloud.Platform.Utils.DecayCache;
using Kusto.Data.Common;
using Kusto.Ingest;
using Kustox.KustoState.DataObjects;
using Kustox.Runtime;
using Kustox.Runtime.State.RunStep;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

namespace Kustox.KustoState
{
    internal class KustoProcedureRunStepStore : IProcedureRunStepStore
    {
        private const string TABLE_NAME = "RunStep";
        private const string PROJECT_CLAUSE =
            "| project JobId, Breadcrumb, State, Script, CaptureName, IsResultScalar, ResultColumnNames, ResultColumnTypes, ResultData, Timestamp";

        private readonly ConnectionProvider _connectionProvider;
        private readonly string _jobId;

        public KustoProcedureRunStepStore(ConnectionProvider connectionProvider, string jobId)
        {
            _connectionProvider = connectionProvider;
            _jobId = jobId;
        }

        string IProcedureRunStepStore.JobId => _jobId;

        async Task<IImmutableList<ProcedureRunStep>> IProcedureRunStepStore.GetAllStepsAsync(
            CancellationToken ct)
        {
            var script = $@"
RunStep
| where JobId=='{_jobId}'
| summarize arg_max(Timestamp,*) by JobId, BreadcrumbId=tostring(Breadcrumb)
| where isnotempty(JobId)";
            var stepsData = await KustoHelper.QueryAsync<StepData>(
                _connectionProvider.QueryProvider,
                script,
                ct);
            var steps = stepsData
                .Select(s => s.ToImmutable())
                .ToImmutableArray();

            return steps;
        }

        async Task<TableResult> IProcedureRunStepStore.QueryStepsAsync(
            string? query,
            IImmutableList<int>? stepBreadcrumb,
            CancellationToken ct)
        {
            var breadcrumbFilter = stepBreadcrumb == null
                ? string.Empty
                : $"| where BreadcrumbId==dynamic([{string.Join(',', stepBreadcrumb)}])";
            var script = $@"
RunStep
| where JobId=='{_jobId}'
| extend BreadcrumbId=tostring(Breadcrumb)
{breadcrumbFilter}
| summarize arg_max(Timestamp,*) by JobId, BreadcrumbId
| where isnotempty(JobId)
| order by Timestamp asc
{PROJECT_CLAUSE}
{query}";
            var stepsData = await _connectionProvider.QueryProvider.ExecuteQueryAsync(
                string.Empty,
                script,
                _connectionProvider.EmptyClientRequestProperties,
                ct);
            var table = stepsData.ToDataSet().Tables[0].ToTableResult();

            return table;
        }

        async Task<TableResult> IProcedureRunStepStore.QueryRunResultAsync(
            string? query,
            CancellationToken ct)
        {
            var stepQuery = $@"Run
| where JobId=='{_jobId}'
| where State=='Completed'
| project JobId
| join kind=inner RunStep on JobId
| where array_length(Breadcrumb)==1
| extend StepIndex=tolong(Breadcrumb[0])
| summarize arg_max(Timestamp, *) by StepIndex
| summarize arg_max(StepIndex, *)
| where State=='Completed'
| where isnotempty(JobId)
{PROJECT_CLAUSE}";
            var stepsData = await KustoHelper.QueryAsync<StepData>(
                _connectionProvider.QueryProvider,
                stepQuery,
                ct);
            var steps = stepsData
                .Select(s => s.ToImmutable())
                .ToImmutableArray();
            var step = steps.LastOrDefault();

            if(step == null)
            {
                return TableResult.CreateEmpty("NoData", string.Empty);
            }
            else
            {
                var resultQuery = $@"RunStep
| where JobId=='{_jobId}'
| where array_length(Breadcrumb)==1
| extend StepIndex=tolong(Breadcrumb[0])
| summarize arg_max(Timestamp, *) by StepIndex
| summarize arg_max(StepIndex, *)
| where State=='Completed'
| where isnotempty(JobId)
| mv-expand ResultData
| project {step.Result!.ToDynamicProjection("ResultData")}
{query}";
                var resultData = await _connectionProvider.QueryProvider.ExecuteQueryAsync(
                    string.Empty,
                    resultQuery,
                    _connectionProvider.EmptyClientRequestProperties,
                    ct);
                var table = resultData.ToDataSet().Tables[0].ToTableResult();

                return table;
            }
        }

        async Task<TableResult> IProcedureRunStepStore.QueryStepResultAsync(
            string? query,
            IImmutableList<int> stepBreadcrumb,
            CancellationToken ct)
        {
            var stepsData = await KustoHelper.QueryAsync<StepData>(
                _connectionProvider.QueryProvider,
                $@"RunStep
| where JobId=='{_jobId}'
| where array_length(Breadcrumb)=={stepBreadcrumb.Count}
| extend BreadcrumbId=tostring(Breadcrumb)
| where BreadcrumbId=='[{string.Join(",", stepBreadcrumb.Select(i => i.ToString()))}]'
| summarize arg_max(Timestamp, *)
| where isnotempty(JobId)",
                ct);
            var steps = stepsData
                .Select(s => s.ToImmutable())
                .ToImmutableArray();
            var step = steps.LastOrDefault();

            if (step?.State != StepState.Completed)
            {
                return null;
            }
            else
            {
                return step.Result;
            }
        }

        async Task IProcedureRunStepStore.AppendStepAsync(
            IEnumerable<ProcedureRunStep> steps,
            CancellationToken ct)
        {
            var data = steps
                .Select(s => new StepData(_jobId, s));

            await KustoHelper.StreamIngestAsync(
                _connectionProvider.StreamingIngestClient,
                _connectionProvider.QueryProvider.DefaultDatabaseName,
                TABLE_NAME,
                data,
                ct);
        }
    }
}