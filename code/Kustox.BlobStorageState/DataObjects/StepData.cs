﻿using Kustox.Compiler;
using Kustox.Runtime.State;
using Kustox.Runtime.State.Run;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.BlobStorageState.DataObjects
{
    internal class StepData
    {
        public StepData()
        {
        }

        public StepData(
            string jobId,
            IImmutableList<long> breadcrumb,
            StepState state,
            string script,
            string? captureName,
            TableData? result)
        {
            JobId = jobId.ToString();
            Breadcrumb = breadcrumb;
            State = state;
            Script = script;
            CaptureName = captureName;
            Result = result;
        }

        public string JobId { get; set; } = string.Empty;

        public StepState State { get; set; }

        public string Script { get; set; } = string.Empty;

        public IImmutableList<long> Breadcrumb { get; set; } = ImmutableArray<long>.Empty;

        public string? CaptureName { get; set; }

        public TableData? Result { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool HasBreadcrumbPrefix(IImmutableList<long> levelPrefix)
        {
            if (levelPrefix.Count > Breadcrumb.Count)
            {
                return false;
            }

            for (int i = 0; i != levelPrefix.Count; ++i)
            {
                if (Breadcrumb[i] != levelPrefix[i])
                {
                    return false;
                }
            }

            return true;
        }

        public ProcedureRunStep ToControlFlowStep()
        {
            if (Result != null)
            {
                var columns = Result!.ColumnNames!
                    .Zip(Result!.ColumnTypes!, (n, t) => new ColumnSpecification(n, t))
                    .ToImmutableArray();

                return new ProcedureRunStep(
                    Script,
                    Breadcrumb,
                    State,
                    CaptureName,
                    new TableResult(Result!.IsScalar, columns, Result!.Data!),
                    Timestamp);
            }
            else
            {
                return new ProcedureRunStep(
                    Script,
                    Breadcrumb,
                    State,
                    Timestamp);
            }
        }
    }
}