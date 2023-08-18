using Kustox.Compiler;
using Kustox.CosmosDbState.DataObjects;
using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.CosmosDbState
{
    internal class CosmosDbProcedureRun : IProcedureRun
    {
        private readonly Container _container;
        private readonly long _jobId;

        public CosmosDbProcedureRun(Container container, long jobId)
        {
            _container = container;
            _jobId = jobId;
        }

        long IProcedureRun.JobId => _jobId;

        async Task IProcedureRun.CreateInstanceAsync(
            string script,
            CancellationToken ct)
        {
            var declarationData = new DeclarationData(_jobId, script);
            var stateData = new ProcedureRunStateData(_jobId, ProcedureRunState.Running);
            var batch = _container.CreateTransactionalBatch(
                ProcedureRunDataHelper.JobIdToPartitionKey(_jobId));

            batch.CreateItem(declarationData);
            batch.CreateItem(stateData);
            await batch.ExecuteAsync(ct);
        }

        async Task<ProcedureDeclaration> IProcedureRun.GetDeclarationAsync(
            CancellationToken ct)
        {
            var response = await _container.ReadItemAsync<DeclarationData>(
                DeclarationData.GetId(_jobId),
                ProcedureRunDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
            var script = response.Resource.Script;
            var declaration = new KustoxCompiler().CompileScript(script);

            if (declaration == null)
            {
                throw new InvalidDataException($"No declaration for job ID '{_jobId}'");
            }

            return declaration;
        }

        async Task<TimestampedData<ProcedureRunState>> IProcedureRun.GetControlFlowStateAsync(
            CancellationToken ct)
        {
            var response = await _container.ReadItemAsync<ProcedureRunStateData>(
                ProcedureRunStateData.GetId(_jobId),
                ProcedureRunDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
            var stateData = response.Resource;

            if (stateData == null)
            {
                throw new InvalidDataException($"No control flow state data for job ID '{_jobId}'");
            }

            var result = TimestampedData.Create(stateData.GetState(), response.Resource._ts);

            return result;
        }

        async Task IProcedureRun.SetControlFlowStateAsync(
            ProcedureRunState state,
            CancellationToken ct)
        {
            var data = new ProcedureRunStateData(_jobId, state);

            await _container.UpsertItemAsync(
                data,
                ProcedureRunDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
        }

        async Task<IImmutableList<ProcedureRunStep>> IProcedureRun.GetStepsAsync(
            IImmutableList<long> levelPrefix,
            CancellationToken ct)
        {
            var query = $@"SELECT *
FROM c
WHERE
ARRAY_LENGTH(c.indexes)={levelPrefix.Count() + 1}
AND STARTSWITH(c.id, '{StepData.GetId(_jobId, levelPrefix)}', false)";
            var iterator = _container.GetItemQueryIterator<StepData>(
                new QueryDefinition(query),
                null,
                new QueryRequestOptions
                {
                    PartitionKey = ProcedureRunDataHelper.JobIdToPartitionKey(_jobId)
                });
            var stepDatas = await ListAsync(iterator, ct);
            var steps = stepDatas
                .Select(d => d.ToControlFlowStep())
                .OrderBy(s => s.StepBreadcrumb.LastOrDefault())
                .ToImmutableArray();

            return steps;
        }

        async Task<ProcedureRunStep> IProcedureRun.SetStepAsync(
            IImmutableList<long> indexes,
            StepState state,
            string script,
            string? captureName,
            TableResult? result,
            CancellationToken ct)
        {
            var data = new StepData(
                _jobId,
                indexes,
                state,
                script,
                captureName,
                result == null ? null : new TableData(result));

            await _container.UpsertItemAsync(
                data,
                ProcedureRunDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);

            return data.ToControlFlowStep();
        }

        private async Task<IImmutableList<T>> ListAsync<T>(
            FeedIterator<T> iterator,
            CancellationToken ct)
        {
            var list = ImmutableArray<T>.Empty.ToBuilder();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(ct);

                list.AddRange(response);
            }

            return list.ToImmutable();
        }
    }
}