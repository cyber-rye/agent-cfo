using AgentCfo.Core.Entities;

namespace AgentCfo.Core.Interfaces;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken ct = default);
}
