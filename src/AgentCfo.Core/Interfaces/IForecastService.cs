using AgentCfo.Core.Entities;

namespace AgentCfo.Core.Interfaces;

public interface IForecastService
{
    Task<Forecast?> GenerateForecastAsync(Guid organizationId, string? scenario = null, CancellationToken ct = default);
}
