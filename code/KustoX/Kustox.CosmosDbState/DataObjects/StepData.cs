using Kustox.Compiler;
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
            IImmutableList<int> indexes,
            string captureName,
            bool isScalarCapture,
            DataTable captureTable)
        {
            Id = GetId(jobId, indexes);
            JobId = jobId.ToString();
            Indexes = indexes;
            CaptureName = captureName;
            IsScalarCapture = isScalarCapture;
            CaptureTable = captureTable;
        }

        public static string GetId(long jobId, IImmutableList<int> indexes)
        {
            var id = $"{jobId}.step.{string.Join(".", indexes)}";

            return id;
        }

        public string Id { get; set; } = string.Empty;

        public string JobId { get; set; } = string.Empty;

        public IImmutableList<int> Indexes { get; set; } = ImmutableArray<int>.Empty;

        public string? CaptureName { get; set; }

        public bool? IsScalarCapture { get; set; }

        public DataTable? CaptureTable { get; set; }
    }
}