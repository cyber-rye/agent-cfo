using AgentCfo.Application.Forecasts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public ForecastsController(IMediator mediator) => _mediator = mediator;
    
    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetCurrentForecast(Guid organizationId, [FromQuery] string? scenario = null)
    {
        var result = await _mediator.Send(new GetCurrentForecast.Query(organizationId, scenario));
        if (result is null) return NotFound();
        return Ok(result);
    }
}
