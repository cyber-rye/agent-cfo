using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IReadOnlyList<Budget>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<Budget?> GetByOrganizationAndCategoryAsync(Guid organizationId, ExpenseCategory category, CancellationToken ct = default);
}
