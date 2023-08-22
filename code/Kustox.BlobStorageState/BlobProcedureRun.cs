using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Kustox.BlobStorageState.DataObjects;
using Kustox.Compiler;
using Kustox.Runtime.State;
using System.Collections.Immutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Kustox.BlobStorageState
{
    internal class BlobProcedureRun : IProcedureRun
    {
        private readonly JsonLogBlob<StepData> _logBlob;
        private readonly long _jobId;

        public BlobProcedureRun(
            DataLakeDirectoryClient rootFolder,
            BlobContainerClient containerClient,
            long jobId)
        {
            _logBlob = new JsonLogBlob<StepData>(
                rootFolder,
                containerClient,
                "log.json",
                Compact);
            _jobId = jobId;
        }

        long IProcedureRun.JobId => _jobId;

        async Task IProcedureRun.CreateRunAsync(string script, CancellationToken ct)
        {
            var data = new StepData(
                _jobId,
                ImmutableArray<long>.Empty,
                StepState.Running,
                script,
                null,
                null);

            await _logBlob.AppendAsync(ImmutableArray.Create(data), ct);
        }

        Task<TimestampedData<ProcedureRunState>> IProcedureRun.GetControlFlowStateAsync(
            CancellationToken ct)
        {
            return Task.FromResult(
                new TimestampedData<ProcedureRunState>(ProcedureRunState.Running, DateTime.Now));
        }

        async Task<ProcedureDeclaration> IProcedureRun.GetDeclarationAsync(CancellationToken ct)
        {
            var data = await _logBlob.ReadAllAsync(ct);
            var declarationNodes = data.Where(d => !d.Breadcrumb.Any());

            if (declarationNodes.Count() != 1)
            {
                throw new InvalidDataException(
                    $"Should only have one declaration node but have {declarationNodes.Count()}");
            }

            var declarationNode = declarationNodes.First();
            var script = declarationNode.Script;
            var declaration = new KustoxCompiler().CompileScript(script);

            if (declaration == null)
            {
                throw new InvalidDataException($"No declaration for job ID '{_jobId}'");
            }

            return declaration;
        }

        async Task<IImmutableList<ProcedureRunStep>> IProcedureRun.GetStepsAsync(
            IImmutableList<long> levelPrefix,
            CancellationToken ct)
        {
            var data = await _logBlob.ReadAllAsync(ct);
            var stepsData = data
                .Where(d => d.Breadcrumb.Count() == levelPrefix.Count() + 1)
                .Where(d => d.HasBreadcrumbPrefix(levelPrefix));
            var steps = stepsData
                .Select(d => d.ToControlFlowStep())
                .OrderBy(s => s.StepBreadcrumb.LastOrDefault())
                .ToImmutableArray();

            return steps;
        }

        async Task IProcedureRun.SetControlFlowStateAsync(
            ProcedureRunState state,
            CancellationToken ct)
        {
            await Task.CompletedTask;
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

            await _logBlob.AppendAsync(ImmutableArray.Create(data), ct);

            return data.ToControlFlowStep();
        }

        private static IImmutableList<StepData> Compact(IEnumerable<StepData> raw)
        {
            var compacted = raw
                .GroupBy(r => string.Join('.', r.Breadcrumb))
                .Select(g => g.ArgMaxBy(r => r.Timestamp)!)
                .ToImmutableArray();

            return compacted;
        }
    }
}