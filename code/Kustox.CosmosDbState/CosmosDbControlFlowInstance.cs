using Kustox.Compiler;
using Kustox.CosmosDbState.DataObjects;
using Kustox.Runtime.State;
using Microsoft.Azure.Cosmos;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.CosmosDbState
{
    internal class CosmosDbControlFlowInstance : IControlFlowInstance
    {
        private readonly Container _container;
        private readonly long _jobId;

        public CosmosDbControlFlowInstance(Container container, long jobId)
        {
            _container = container;
            _jobId = jobId;
        }

        long IControlFlowInstance.JobId => _jobId;

        async Task IControlFlowInstance.CreateInstanceAsync(
            string script,
            CancellationToken ct)
        {
            var declarationData = new DeclarationData(_jobId, script);
            var stateData = new ControlFlowStateData(_jobId, ControlFlowState.Running);
            var batch = _container.CreateTransactionalBatch(
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId));

            batch.CreateItem(declarationData);
            batch.CreateItem(stateData);
            await batch.ExecuteAsync(ct);
        }

        Task IControlFlowInstance.DeleteAsync(CancellationToken ct)
        {
            //  See https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/how-to-delete-by-partition-key?tabs=dotnet-example
            //  For preview
            //await _container.DeleteAllItemsByPartitionKeyStreamAsync(ct);
            throw new NotImplementedException();
        }

        async Task<ControlFlowDeclaration> IControlFlowInstance.GetDeclarationAsync(
            CancellationToken ct)
        {
            var response = await _container.ReadItemAsync<DeclarationData>(
                DeclarationData.GetId(_jobId),
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
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

        async Task<TimestampedData<ControlFlowState>> IControlFlowInstance.GetControlFlowStateAsync(
            CancellationToken ct)
        {
            var response = await _container.ReadItemAsync<ControlFlowStateData>(
                ControlFlowStateData.GetId(_jobId),
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
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

        async Task IControlFlowInstance.SetControlFlowStateAsync(
            ControlFlowState state,
            CancellationToken ct)
        {
            var data = new ControlFlowStateData(_jobId, state);

            await _container.UpsertItemAsync(
                data,
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
                null,
                ct);
        }

        async Task<IImmutableList<ControlFlowStep>> IControlFlowInstance.GetStepsAsync(
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
                    PartitionKey = ControlFlowDataHelper.JobIdToPartitionKey(_jobId)
                });
            var stepDatas = await ListAsync(iterator, ct);
            var steps = stepDatas
                .Select(d => d.ToControlFlowStep())
                .OrderBy(s => s.StepBreadcrumb.LastOrDefault())
                .ToImmutableArray();

            return steps;
        }

        async Task<ControlFlowStep> IControlFlowInstance.SetStepAsync(
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
                ControlFlowDataHelper.JobIdToPartitionKey(_jobId),
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