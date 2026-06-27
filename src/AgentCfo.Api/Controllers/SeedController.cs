using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IOrganizationRepository _orgRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IBudgetRepository _budgetRepo;
    private readonly IAgentDecisionRepository _decisionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SeedController(
        IOrganizationRepository orgRepo,
        ITransactionRepository transactionRepo,
        IBudgetRepository budgetRepo,
        IAgentDecisionRepository decisionRepo,
        IUnitOfWork unitOfWork)
    {
        _orgRepo = orgRepo;
        _transactionRepo = transactionRepo;
        _budgetRepo = budgetRepo;
        _decisionRepo = decisionRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("demo")]
    public async Task<IActionResult> SeedDemoData(CancellationToken ct)
    {
        // Create demo organization
        var org = Organization.Create("Acme SaaS", "cus_demo_acme", Money.From(50000, "USD"));
        await _orgRepo.AddAsync(org, ct);

        var orgId = org.Id;
        var random = new Random(42);
        var now = DateTime.UtcNow;

        // Seed 3 months of transaction history with realistic dates
        var transactions = new List<Transaction>();

        // Revenue: monthly subscriptions growing from ~$12K to ~$18K
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

            // 15-25 subscription payments per month
            var subCount = 15 + random.Next(0, 10);
            for (int i = 0; i < subCount; i++)
            {
                var amount = (decimal)(49 + random.Next(0, 200));
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 22), random.Next(0, 60), 0, DateTimeKind.Utc);

                transactions.Add(Transaction.CreateRevenue(
                    orgId, Money.From(amount, "USD"),
                    $"Subscription payment - Plan {(amount > 150 ? "Pro" : "Starter")}",
                    $"evt_sub_{month}_{i}",
                    $"pi_{Guid.NewGuid():N}",
                    date));
            }

            // A few refunds (1-2 per month)
            var refundCount = random.Next(0, 3);
            for (int i = 0; i < refundCount; i++)
            {
                var amount = (decimal)(49 + random.Next(0, 100));
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 22), random.Next(0, 60), 0, DateTimeKind.Utc);

                transactions.Add(Transaction.CreateRefund(
                    orgId, Money.From(amount, "USD"),
                    "Refund - Customer churn",
                    null,
                    date));
            }
        }

        // Expenses
        var expenseCategories = new[]
        {
            (ExpenseCategory.Infrastructure, "AWS Cloud Hosting", 2100m),
            (ExpenseCategory.Tools, "GitHub Enterprise", 800m),
            (ExpenseCategory.Tools, "Slack Business+", 350m),
            (ExpenseCategory.Tools, "Figma Professional", 180m),
            (ExpenseCategory.Marketing, "Google Ads", 1500m),
            (ExpenseCategory.Marketing, "LinkedIn Ads", 800m),
            (ExpenseCategory.Contractors, "Design Agency Retainer", 3000m),
            (ExpenseCategory.Office, "Coworking Space", 1200m),
            (ExpenseCategory.Infrastructure, "Vercel Pro", 200m),
            (ExpenseCategory.Tools, "Notion Team", 120m),
        };

        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

            foreach (var (category, desc, baseAmount) in expenseCategories)
            {
                var amount = baseAmount * (1 + (decimal)(random.NextDouble() * 0.1 - 0.05));
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 18), random.Next(0, 60), 0, DateTimeKind.Utc);

                transactions.Add(Transaction.CreateExpense(
                    orgId, Money.From(Math.Round(amount, 2), "USD"), desc, category, null, date));
            }
        }

        foreach (var tx in transactions)
            await _transactionRepo.AddAsync(tx, ct);

        // Seed budgets
        var budgets = new[]
        {
            Budget.Create(orgId, ExpenseCategory.Infrastructure, Money.From(3000, "USD"), true, 80),
            Budget.Create(orgId, ExpenseCategory.Marketing, Money.From(3000, "USD"), false, 75),
            Budget.Create(orgId, ExpenseCategory.Tools, Money.From(2000, "USD"), false, 80),
            Budget.Create(orgId, ExpenseCategory.Contractors, Money.From(5000, "USD"), false, 70),
            Budget.Create(orgId, ExpenseCategory.Office, Money.From(1500, "USD"), true, 90),
        };

        foreach (var budget in budgets)
            await _budgetRepo.AddAsync(budget, ct);

        // Seed agent decisions with realistic timestamps
        var decisions = new[]
        {
            AgentDecision.Propose(orgId, AgentDecisionType.AnomalyDetected,
                "Infrastructure spend increased 23% over past 30 days",
                "Cloud hosting costs grew from $2,100 to $2,583. Pattern suggests instance sizing may need review. " +
                "Projected annual impact: +$5,796. Recommend reviewing reserved instances or right-sizing."),
            AgentDecision.Propose(orgId, AgentDecisionType.ExpenseDenied,
                "Marketing analytics tool request denied",
                "Evaluating expense request: $3,000/yr marketing analytics tool. Marketing budget remaining this month: $4,200. " +
                "Current tool stack includes similar capability. Recommend: DENY — duplicate capability. " +
                "If approved, would consume 71% of remaining monthly marketing budget."),
            AgentDecision.Propose(orgId, AgentDecisionType.AlertRaised,
                "Runway warning: 8.3 months remaining",
                "Current burn rate: $15,200/mo. Current revenue: $18,400/mo. Net positive cash flow: $3,200/mo. " +
                "However, committed hiring plan (2 engineers) would increase burn to $27,200/mo, reducing runway to 6.1 months. " +
                "Recommend delaying hires until MRR reaches $25K."),
            AgentDecision.Propose(orgId, AgentDecisionType.ReportGenerated,
                "Monthly financial summary generated",
                "June 2026 financial summary: MRR $18,400 (+15% MoM), Total expenses $15,200, Net income $3,200. " +
                "Key metrics: LTV $1,200, CAC payback 4.2 months, Gross margin 82%. " +
                "All metrics trending positively. Company is fundraise-ready at current trajectory."),
        };

        foreach (var decision in decisions)
            await _decisionRepo.AddAsync(decision, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Ok(new
        {
            OrganizationId = orgId,
            OrganizationName = "Acme SaaS",
            TransactionCount = transactions.Count,
            BudgetCount = budgets.Length,
            DecisionCount = decisions.Length,
            Message = "Demo data seeded successfully"
        });
    }
}
