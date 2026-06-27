using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentController : ControllerBase
{
    private readonly IAgentDecisionRepository _decisionRepo;
    private readonly IAuditEntryRepository _auditRepo;

    public AgentController(IAgentDecisionRepository decisionRepo, IAuditEntryRepository auditRepo)
    {
        _decisionRepo = decisionRepo;
        _auditRepo = auditRepo;
    }

    [HttpGet("{organizationId:guid}/decisions")]
    public async Task<IActionResult> GetDecisions(Guid organizationId, [FromQuery] int limit = 20)
    {
        var decisions = await _decisionRepo.GetRecentAsync(organizationId, limit);
        return Ok(decisions.Select(d => new
        {
            d.Id,
            Type = d.Type.ToString(),
            d.Description,
            d.Reasoning,
            Status = d.Status.ToString(),
            d.CreatedAt,
            d.ExecutedAt
        }));
    }

    [HttpGet("{organizationId:guid}/audit")]
    public async Task<IActionResult> GetAuditTrail(Guid organizationId, [FromQuery] int limit = 50)
    {
        var entries = await _auditRepo.GetByOrganizationAsync(organizationId, limit);
        return Ok(entries.Select(a => new
        {
            a.Id,
            Actor = a.ActorType.ToString(),
            a.ActorId,
            a.Action,
            a.EntityType,
            a.EntityId,
            a.CreatedAt,
            a.CorrelationId
        }));
    }
}
