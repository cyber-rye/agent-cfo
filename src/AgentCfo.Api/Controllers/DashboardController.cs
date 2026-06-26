using AgentCfo.Application.Forecasts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public DashboardController(IMediator mediator) => _mediator = mediator;
    
    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetSummary(Guid organizationId)
    {
        var result = await _mediator.Send(new GetDashboardSummary.Query(organizationId));
        return Ok(result);
    }
}
