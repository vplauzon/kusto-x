using Kustox.Compiler;
using Kustox.Runtime.State;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.CosmosDbState.DataObjects
{
    internal class StepData
    {
        public StepData()
        {
        }

        public StepData(
            long jobId,
            IImmutableList<long> indexes,
            int retry,
            StepState state,
            string? captureName,
            bool? isScalarCapture,
            DataTable? result)
        {
            Id = GetId(jobId, indexes);
            JobId = jobId.ToString();
            State = state.ToString();
            Retry = retry;
            Indexes = indexes;
            CaptureName = captureName;
            Result = result == null ? null : new TableData(result, isScalarCapture ?? false);
        }

        public static string GetIdPrefix(long jobId)
        {
            var prefix = $"{jobId}.step";

            return prefix;
        }

        public static string GetId(long jobId, IImmutableList<long> indexes)
        {
            var id = $"{GetIdPrefix(jobId)}.{string.Join(".", indexes)}";

            return id;
        }

        public string Id { get; set; } = string.Empty;

        public string JobId { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public int Retry { get; set; } = 0;

        public IImmutableList<long> Indexes { get; set; } = ImmutableArray<long>.Empty;

        public string? CaptureName { get; set; }

        public TableData? Result { get; set; }

        public long _ts { get; set; }

        public StepState GetState()
        {
            StepState strongState;

            if (!Enum.TryParse<StepState>(State, out strongState))
            {
                throw new InvalidDataException($"Invalid control state:  '{State}'");
            }
            else
            {
                return strongState;
            }
        }

        public ControlFlowStep ToControlFlowStep()
        {
            if (Result != null)
            {
                var columns = Result!.ColumnNames!
                    .Zip(Result!.ColumnTypes!, (n, t) => new ColumnSpecification(n, t))
                    .ToImmutableArray();

                return new ControlFlowStep(
                    Indexes,
                    GetState(),
                    Retry,
                    CaptureName,
                    new TableResult(Result!.IsScalar, columns, Result!.Data!),
                    _ts);
            }
            else
            {
                return new ControlFlowStep(
                    Indexes,
                    GetState(),
                    Retry,
                    _ts);
            }
        }
    }
}