using AgentCfo.Core.Common;
using AgentCfo.Core.Entities;
using AgentCfo.Core.Enums;
using AgentCfo.Core.Interfaces;
using AgentCfo.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgentCfo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationRepository _orgRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IBudgetRepository _budgetRepo;
    private readonly IAgentDecisionRepository _decisionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SeedController(
        AppDbContext db,
        IOrganizationRepository orgRepo,
        ITransactionRepository transactionRepo,
        IBudgetRepository budgetRepo,
        IAgentDecisionRepository decisionRepo,
        IUnitOfWork unitOfWork)
    {
        _db = db;
        _orgRepo = orgRepo;
        _transactionRepo = transactionRepo;
        _budgetRepo = budgetRepo;
        _decisionRepo = decisionRepo;
        _unitOfWork = unitOfWork;
    }

    [HttpPost("demo")]
    public async Task<IActionResult> SeedDemoData(CancellationToken ct)
    {
        // Clear existing data
        _db.AuditEntries.RemoveRange(_db.AuditEntries);
        _db.AgentDecisions.RemoveRange(_db.AgentDecisions);
        _db.Transactions.RemoveRange(_db.Transactions);
        _db.Budgets.RemoveRange(_db.Budgets);
        _db.Forecasts.RemoveRange(_db.Forecasts);
        _db.Organizations.RemoveRange(_db.Organizations);
        await _db.SaveChangesAsync(ct);

        var org = Organization.Create("NovaCRM", "cus_demo_nova", Money.From(45000, "USD"));
        await _orgRepo.AddAsync(org, ct);
        var orgId = org.Id;
        var random = new Random(42);
        var now = DateTime.UtcNow;

        var transactions = new List<Transaction>();

        // ── Revenue: subscription payments growing ~$18K → $22K over 3 months ──
        var monthlyRevenueTargets = new[] { 18000m, 20000m, 22000m };
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var target = monthlyRevenueTargets[2 - month];
            var subCount = 22 + random.Next(0, 6);
            var avgAmount = target / subCount;

            for (int i = 0; i < subCount; i++)
            {
                // Mix of plan tiers: some Starter ($49-79), some Pro ($149-249), a few Business ($399+)
                var tier = random.NextDouble();
                decimal amount;
                string plan;
                if (tier < 0.5) { amount = 49 + random.Next(0, 30); plan = "Starter"; }
                else if (tier < 0.85) { amount = 149 + random.Next(0, 100); plan = "Pro"; }
                else { amount = 399 + random.Next(0, 200); plan = "Business"; }

                // Scale to hit target
                amount *= (target / (subCount * 150m));

                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(9, 20), random.Next(0, 60), 0, DateTimeKind.Utc);

                transactions.Add(Transaction.CreateRevenue(
                    orgId, Money.From(Math.Round(amount, 2), "USD"),
                    $"Subscription - {plan} Plan",
                    $"evt_sub_{month}_{i}", $"pi_{Guid.NewGuid().ToString("N")[..12]}", date));
            }

            // Occasional refund (0-1 per month)
            if (random.Next(0, 4) == 0)
            {
                var day = random.Next(5, 22);
                var date = new DateTime(monthStart.Year, monthStart.Month, day, 11, 0, 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRefund(
                    orgId, Money.From(79 + random.Next(0, 50), "USD"),
                    "Refund - Customer downgrade", null, date));
            }
        }

        // ── Expenses: ~$12-14K/mo — well below revenue ──
        var expenseCategories = new (ExpenseCategory cat, string desc, decimal baseAmount)[]
        {
            (ExpenseCategory.Infrastructure, "AWS Cloud Hosting", 1800),
            (ExpenseCategory.Infrastructure, "Vercel Pro", 200),
            (ExpenseCategory.Infrastructure, "Cloudflare R2", 80),
            (ExpenseCategory.Tools, "GitHub Team", 200),
            (ExpenseCategory.Tools, "Slack Business+", 250),
            (ExpenseCategory.Tools, "Figma Professional", 150),
            (ExpenseCategory.Tools, "Linear Pro", 120),
            (ExpenseCategory.Tools, "Notion Team", 100),
            (ExpenseCategory.Marketing, "Google Ads", 2000),
            (ExpenseCategory.Marketing, "Content Marketing", 1500),
            (ExpenseCategory.Contractors, "Freelance Designer", 2000),
            (ExpenseCategory.Contractors, "Part-time QA", 1500),
            (ExpenseCategory.Office, "Coworking Space", 800),
            (ExpenseCategory.Legal, "Legal Consultation", 400),
        };

        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

            foreach (var (category, desc, baseAmount) in expenseCategories)
            {
                // Small random variance (±5%)
                var amount = baseAmount * (1 + (decimal)(random.NextDouble() * 0.10 - 0.05));
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 18), random.Next(0, 60), 0, DateTimeKind.Utc);

                transactions.Add(Transaction.CreateExpense(
                    orgId, Money.From(Math.Round(amount, 2), "USD"), desc, category, null, date));
            }
        }

        foreach (var tx in transactions)
            await _transactionRepo.AddAsync(tx, ct);

        // ── Budgets ──
        var budgets = new[]
        {
            Budget.Create(orgId, ExpenseCategory.Infrastructure, Money.From(3000, "USD"), true, 80),
            Budget.Create(orgId, ExpenseCategory.Marketing, Money.From(4000, "USD"), false, 75),
            Budget.Create(orgId, ExpenseCategory.Tools, Money.From(1500, "USD"), false, 80),
            Budget.Create(orgId, ExpenseCategory.Contractors, Money.From(4000, "USD"), false, 70),
            Budget.Create(orgId, ExpenseCategory.Office, Money.From(1200, "USD"), true, 90),
        };

        // Sync budget spend with current month expenses
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var currentMonthExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && t.OccurredAt >= currentMonthStart)
            .ToList();

        foreach (var budget in budgets)
        {
            var categorySpend = currentMonthExpenses
                .Where(t => t.Category == budget.Category)
                .Aggregate(Money.Zero(), (acc, t) => acc.Add(t.Amount));
            budget.RecordSpend(categorySpend);
            await _budgetRepo.AddAsync(budget, ct);
        }

        // ── Agent Decisions ──
        var decisions = new[]
        {
            AgentDecision.Propose(orgId, AgentDecisionType.AnomalyDetected,
                "Infrastructure spend up 12% — within tolerance",
                "Cloud hosting costs grew from $1,800 to $2,020 over 30 days. Growth aligns with increased " +
                "customer count (+28 new subscriptions this month). At current trajectory, infrastructure " +
                "cost per customer is declining from $1.42 to $1.31. No action needed — growth is healthy."),
            AgentDecision.Propose(orgId, AgentDecisionType.ExpenseApproved,
                "Content marketing retainer approved",
                "Evaluating expense request: $1,500/mo content marketing agency. Marketing budget remaining " +
                "this month: $2,500. Current customer acquisition cost: $85. Content marketing projected to " +
                "reduce CAC by 15-20%. Budget impact: 37.5% of monthly allocation. APPROVED — clear ROI."),
            AgentDecision.Propose(orgId, AgentDecisionType.ForecastUpdated,
                "Runway: indefinite — cash flow positive",
                "Cash flow forecast generated. Current balance: $45,000. Monthly burn: $11,200. " +
                "Monthly revenue: $22,000. Net positive cash flow: $10,800/mo. Runway: indefinite at " +
                "current trajectory. If hiring 2 engineers (+$20K/mo burn), runway would be ~14 months. " +
                "Company is in a strong position for strategic growth investments."),
            AgentDecision.Propose(orgId, AgentDecisionType.ReportGenerated,
                "Monthly financial summary — strong growth trajectory",
                "June 2026 financial summary: MRR $22,000 (+10% MoM), Total expenses $11,200, " +
                "Net income $10,800. Key metrics: LTV $1,400, CAC payback 3.8 months, Gross margin 84%. " +
                "Revenue growing faster than expenses. 22 active subscribers across Starter/Pro/Business tiers. " +
                "All metrics trending positively. Recommend accelerating hiring plan."),
        };

        foreach (var decision in decisions)
            await _decisionRepo.AddAsync(decision, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Ok(new
        {
            OrganizationId = orgId,
            OrganizationName = "NovaCRM",
            TransactionCount = transactions.Count,
            BudgetCount = budgets.Length,
            DecisionCount = decisions.Length,
        });
    }
}
