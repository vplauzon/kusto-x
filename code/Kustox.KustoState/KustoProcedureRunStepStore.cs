﻿using Kusto.Cloud.Platform.Utils.DecayCache;
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

        private readonly ConnectionProvider _connectionProvider;
        private readonly string _jobId;

        public KustoProcedureRunStepStore(ConnectionProvider connectionProvider, string jobId)
        {
            _connectionProvider = connectionProvider;
            _jobId = jobId;
        }

        string IProcedureRunStepStore.JobId => _jobId;

        async Task<IImmutableList<ProcedureRunStep>> IProcedureRunStepStore.GetAllLatestStepsAsync(
            CancellationToken ct)
        {
            var stepsData = await KustoHelper.QueryAsync<StepData>(
                _connectionProvider.QueryProvider,
                $@"RunStep
| where JobId=='{_jobId}'
| extend Breadcrumb=tostring(Breadcrumb)
| extend Breadcrumb=tostring(Breadcrumb)
| summarize arg_max(Timestamp,*) by JobId, Breadcrumb
| extend Breadcrumb=todynamic(Breadcrumb)",
                ct);
            var steps = stepsData
                .Select(s => s.ToImmutable())
                .ToImmutableArray();

            return steps;
        }

        Task<TableResult> IProcedureRunStepStore.QueryLatestRunsAsync(
            string? query,
            CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        async Task<TableResult?> IProcedureRunStepStore.GetRunResultAsync(CancellationToken ct)
        {
            var stepsData = await KustoHelper.QueryAsync<StepData>(
                _connectionProvider.QueryProvider,
                $@"RunStep
| where JobId=='{_jobId}'
| where array_length(Breadcrumb)==1
| extend StepIndex=tolong(Breadcrumb[0])
| summarize arg_max(StepIndex, *)",
                ct);
            var steps = stepsData
                .Select(s => s.ToImmutable())
                .ToImmutableArray();

            if (steps.Length != 1)
            {
                throw new InvalidDataException($"Can't find last step for job {_jobId}");
            }

            var lastStep = steps.Last();

            if (lastStep.State != StepState.Completed)
            {
                throw new InvalidDataException($"Requested last step result for job {_jobId} "
                    + $"where status is {lastStep.State}");
            }
            if (lastStep.Result == null)
            {
                throw new NullReferenceException($"Requested last step result for job {_jobId} "
                    + "where no result are present");
            }

            return lastStep.Result;
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