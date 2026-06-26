using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IReadOnlyList<Transaction>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByOrganizationAndTypeAsync(Guid organizationId, TransactionType type, CancellationToken ct = default);
    Task<IReadOnlyList<Transaction>> GetByOrganizationAndDateRangeAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<Money> GetTotalRevenueAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<Money> GetTotalExpensesAsync(Guid organizationId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<Transaction?> GetByStripeEventIdAsync(string stripeEventId, CancellationToken ct = default);
}
