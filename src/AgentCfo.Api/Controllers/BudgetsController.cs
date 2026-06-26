using AgentCfo.Application.Budgets;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public BudgetsController(IMediator mediator) => _mediator = mediator;
    
    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetBudgets(Guid organizationId)
    {
        var result = await _mediator.Send(new GetBudgets.Query(organizationId));
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudget.Command command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBudgets), new { organizationId = command.OrganizationId }, result);
    }
}
