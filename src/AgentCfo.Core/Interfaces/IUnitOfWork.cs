namespace AgentCfo.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
