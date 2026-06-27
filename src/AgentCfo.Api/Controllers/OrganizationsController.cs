using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly IOrganizationRepository _orgRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationsController(IOrganizationRepository orgRepo, IUnitOfWork unitOfWork)
    {
        _orgRepo = orgRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var orgs = await _orgRepo.GetAllAsync();
        return Ok(orgs.Select(o => new
        {
            o.Id,
            o.Name,
            o.StripeCustomerId,
            o.MonthlyBudget,
            o.RunwayThresholdDays,
            o.CreatedAt
        }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var org = await _orgRepo.GetByIdAsync(id);
        if (org is null) return NotFound();
        return Ok(new
        {
            org.Id,
            org.Name,
            org.StripeCustomerId,
            org.MonthlyBudget,
            org.RunwayThresholdDays,
            org.CreatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request)
    {
        var org = Organization.Create(
            request.Name,
            request.StripeCustomerId,
            Money.From(request.MonthlyBudget, request.Currency ?? "USD"),
            request.RunwayThresholdDays);

        await _orgRepo.AddAsync(org);
        await _unitOfWork.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = org.Id }, new
        {
            org.Id,
            org.Name,
            org.StripeCustomerId
        });
    }
}

public record CreateOrganizationRequest(
    string Name,
    string StripeCustomerId,
    decimal MonthlyBudget,
    string? Currency = null,
    int RunwayThresholdDays = 90);
