using AgentCfo.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AgentCfo.Application.Common;

/// <summary>
/// Pipeline behavior that adds structured logging for all MediatR requests.
/// The actual audit trail is handled by AppDbContext.SaveChangesAsync() which
/// automatically creates AuditEntry records for all entity changes.
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

    public AuditBehavior(ILogger<AuditBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogInformation("Audit: Processing {RequestName} ({RequestId})", requestName, requestId);

        try
        {
            var response = await next();
            _logger.LogInformation("Audit: Completed {RequestName} ({RequestId})", requestName, requestId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit: Failed {RequestName} ({RequestId})", requestName, requestId);
            throw;
        }
    }
}
