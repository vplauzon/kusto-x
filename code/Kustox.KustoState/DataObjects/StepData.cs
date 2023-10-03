using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.RunStep;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.KustoState.DataObjects
{
    internal class StepData
    {
        public StepData()
        {
        }

        public StepData(
            string jobId,
            IImmutableList<int> breadcrumb,
            StepState state,
            string script,
            string? captureName,
            bool? isResultScalar,
            IImmutableList<string>? resultColumnNames,
            IImmutableList<string>? resultColumnTypes,
            IImmutableList<IImmutableList<object>>? resultData)
        {
            JobId = jobId;
            Breadcrumb = breadcrumb;
            State = state;
            Script = script;
            CaptureName = captureName;
            IsResultScalar = isResultScalar;
            ResultColumnNames = resultColumnNames;
            ResultColumnTypes = resultColumnTypes;
            ResultData = resultData;
        }

        public StepData(string jobId, ProcedureRunStep step)
            : this(
                  jobId,
                  step.StepBreadcrumb,
                  step.State,
                  step.Script,
                  step.CaptureName,
                  step.Result?.IsScalar,
                  step.Result?.Columns.Select(c => c.ColumnName).ToImmutableArray(),
                  step.Result?.Columns.Select(c => c.ColumnType.FullName).ToImmutableArray(),
                  step.Result?.Data)
        {
        }

        public string JobId { get; set; } = string.Empty;
        
        public StepState State { get; set; }

        public string Script { get; set; } = string.Empty;

        public IImmutableList<int> Breadcrumb { get; set; } = ImmutableArray<int>.Empty;

        public string? CaptureName { get; set; }

        public bool? IsResultScalar { get; set; }
        
        public IImmutableList<string>? ResultColumnNames { get; set; }
        
        public IImmutableList<string>? ResultColumnTypes { get; set; }
        
        public IImmutableList<IImmutableList<object>>? ResultData { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ProcedureRunStep ToImmutable()
        {
            if (IsResultScalar != null)
            {
                var columns = ResultColumnNames!
                    .Zip(ResultColumnTypes!, (n, t) => new ColumnSpecification(n, t))
                    .ToImmutableArray();

                return new ProcedureRunStep(
                    Script,
                    Breadcrumb,
                    State,
                    CaptureName,
                    IsResultScalar.Value
                    ? new TableResult(ResultData![0][0])
                    : new TableResult(columns, ResultData!),
                    Timestamp);
            }
            else
            {
                return new ProcedureRunStep(
                    Script,
                    Breadcrumb,
                    State,
                    null,
                    null,
                    Timestamp);
            }
        }
    }
}