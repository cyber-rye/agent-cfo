using AgentCfo.Application.Transactions;
using AgentCfo.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public TransactionsController(IMediator mediator) => _mediator = mediator;
    
    [HttpGet("{organizationId:guid}")]
    public async Task<IActionResult> GetTransactions(Guid organizationId, [FromQuery] TransactionType? type = null, [FromQuery] int limit = 50)
    {
        var result = await _mediator.Send(new GetTransactions.Query(organizationId, type, limit));
        return Ok(result);
    }
    
    [HttpPost("expense")]
    public async Task<IActionResult> RecordExpense([FromBody] RecordExpense.Command command)
    {
        var result = await _mediator.Send(command);
        if (!result.Approved)
            return Conflict(new { result.TransactionId, result.RejectionReason });
        return Ok(result);
    }
}
