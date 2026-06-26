using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;

namespace AgentCfo.Core.Interfaces;

public interface IAgentService
{
    Task<AgentDecision> AnalyzeTransactionAsync(Transaction transaction, CancellationToken ct = default);
    Task<AgentDecision> EvaluateExpenseRequestAsync(Guid organizationId, Money amount, string description, CancellationToken ct = default);
    Task<Forecast> GenerateForecastAsync(Guid organizationId, string? scenario = null, CancellationToken ct = default);
    Task<AgentDecision> DetectAnomalyAsync(Guid organizationId, CancellationToken ct = default);
    Task<string> GenerateFinancialSummaryAsync(Guid organizationId, CancellationToken ct = default);
}
