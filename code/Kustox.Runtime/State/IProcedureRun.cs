﻿using Kustox.Compiler;
using System.Collections.Immutable;
using System.Data;

namespace Kustox.Runtime.State
{
    public interface IProcedureRun
    {
        long JobId { get; }

        Task CreateInstanceAsync(string script, CancellationToken ct);
        
        Task<ProcedureDeclaration> GetDeclarationAsync(CancellationToken ct);
        
        Task<TimestampedData<ProcedureRunState>> GetControlFlowStateAsync(CancellationToken ct);
        
        Task<IImmutableList<ProcedureRunStep>> GetStepsAsync(
            IImmutableList<long> levelPrefix,
            CancellationToken ct);
        
        Task SetControlFlowStateAsync(ProcedureRunState state, CancellationToken ct);

        Task<ProcedureRunStep> SetStepAsync(
            IImmutableList<long> indexes,
            StepState state,
            string script,
            string? captureName,
            TableResult? result,
            CancellationToken ct);
    }
}