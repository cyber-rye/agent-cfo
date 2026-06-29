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
    public async Task<IActionResult> SeedDemoData([FromQuery] string? profile = null, CancellationToken ct = default)
    {
        var seed = new Random().Next(1000, 9999);
        return profile?.ToLower() switch
        {
            "cash-crunch" => await SeedCashCrunch(seed, ct),
            "hypergrowth" => await SeedHypergrowth(seed, ct),
            "pre-seed" => await SeedPreSeed(seed, ct),
            _ => await SeedGrowthSaas(seed, ct),
        };
    }

    // ── Profile 1: Growth SaaS (healthy, positive outlook) ───────────
    private async Task<IActionResult> SeedGrowthSaas(int seed, CancellationToken ct)
    {
        var org = Organization.Create("NovaCRM", $"cus_{seed}_nova", Money.From(50000, "USD"));
        await _orgRepo.AddAsync(org, ct);
        var orgId = org.Id;
        var random = new Random(seed);
        var now = DateTime.UtcNow;

        var transactions = new List<Transaction>();

        // Revenue: subscriptions growing from ~$18K to ~$24K over 3 months
        var monthlySubBase = new[] { 18000m, 20500m, 23000m };
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var targetRevenue = monthlySubBase[2 - month];
            var subCount = 20 + random.Next(0, 8);
            var avgAmount = targetRevenue / subCount;

            for (int i = 0; i < subCount; i++)
            {
                var amount = avgAmount * (decimal)(0.6 + random.NextDouble() * 0.8);
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 22), random.Next(0, 60), 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRevenue(
                    orgId, Money.From(Math.Round(amount, 2), "USD"),
                    $"Subscription - Plan {(amount > 120 ? "Business" : "Starter")}",
                    $"evt_sub_{month}_{i}", $"pi_{Guid.NewGuid():N[..12]}", date));
            }

            // 0-1 refunds per month
            if (random.Next(0, 3) == 0)
            {
                var day = random.Next(1, 20);
                var date = new DateTime(monthStart.Year, monthStart.Month, day, 10, 0, 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRefund(orgId, Money.From(79 + random.Next(0, 80), "USD"),
                    "Refund - Customer churn", null, date));
            }
        }

        // Expenses: ~$14-16K/mo — manageable
        var expenses = new (ExpenseCategory, string, decimal)[]
        {
            (ExpenseCategory.Infrastructure, "AWS Cloud Hosting", 2200),
            (ExpenseCategory.Infrastructure, "Vercel Pro", 200),
            (ExpenseCategory.Tools, "GitHub Team", 400),
            (ExpenseCategory.Tools, "Slack Business+", 350),
            (ExpenseCategory.Tools, "Figma Professional", 180),
            (ExpenseCategory.Tools, "Notion Team", 120),
            (ExpenseCategory.Tools, "Linear Pro", 160),
            (ExpenseCategory.Marketing, "Google Ads", 2000),
            (ExpenseCategory.Marketing, "Content Marketing Agency", 1500),
            (ExpenseCategory.Contractors, "Freelance Backend Dev", 3500),
            (ExpenseCategory.Office, "Coworking Space", 1200),
            (ExpenseCategory.Legal, "Legal Consultation", 500),
        };
        SeedExpenses(orgId, transactions, expenses, random, now);

        foreach (var tx in transactions)
            await _transactionRepo.AddAsync(tx, ct);

        // Budgets
        var budgets = new[]
        {
            Budget.Create(orgId, ExpenseCategory.Infrastructure, Money.From(3000, "USD"), true, 80),
            Budget.Create(orgId, ExpenseCategory.Marketing, Money.From(4000, "USD"), false, 75),
            Budget.Create(orgId, ExpenseCategory.Tools, Money.From(2000, "USD"), false, 80),
            Budget.Create(orgId, ExpenseCategory.Contractors, Money.From(5000, "USD"), false, 70),
            Budget.Create(orgId, ExpenseCategory.Office, Money.From(1500, "USD"), true, 90),
        };
        await SeedBudgets(budgets, transactions, now, ct);

        // Agent decisions — positive story
        var decisions = new[]
        {
            AgentDecision.Propose(orgId, AgentDecisionType.AnomalyDetected,
                "Infrastructure spend up 12% — within tolerance",
                "Cloud hosting costs grew from $2,100 to $2,350 over 30 days. Growth aligns with increased customer count (+18 new subscriptions). " +
                "At current trajectory, infrastructure cost per customer is declining. No action needed — growth is healthy."),
            AgentDecision.Propose(orgId, AgentDecisionType.ExpenseApproved,
                "Content marketing agency retainer approved",
                "Evaluating expense request: $1,500/mo content marketing agency. Marketing budget remaining: $2,500. " +
                "Current customer acquisition cost: $85. Content marketing projected to reduce CAC by 15-20%. " +
                "Budget impact: 37.5% of monthly marketing allocation. APPROVED — strategic investment with clear ROI."),
            AgentDecision.Propose(orgId, AgentDecisionType.ForecastUpdated,
                "Runway: 14.2 months — healthy position",
                "Cash flow forecast generated. Current balance: $52,400. Monthly burn: $15,200. Monthly revenue: $23,000. " +
                "Net positive cash flow: $7,800/mo. Runway: indefinite at current trajectory. " +
                "If hiring 2 engineers (+$20K/mo burn), runway would be ~11 months. Company is fundraise-ready."),
            AgentDecision.Propose(orgId, AgentDecisionType.ReportGenerated,
                "Q2 2026 financial summary — strong growth",
                "Monthly financial summary: MRR $23,000 (+12% MoM), Total expenses $15,200, Net income $7,800. " +
                "Key metrics: LTV $1,400, CAC payback 3.8 months, Gross margin 84%. " +
                "Revenue growing faster than expenses. All metrics trending positively. Recommend accelerating hiring plan."),
        };
        await SeedDecisions(decisions, ct);

        return Ok(new
        {
            OrganizationId = orgId,
            Profile = "growth-saas",
            OrganizationName = "NovaCRM",
            TransactionCount = transactions.Count,
            BudgetCount = budgets.Length,
            DecisionCount = decisions.Length,
            Message = "Growth SaaS profile seeded — NovaCRM, $23K MRR, healthy outlook"
        });
    }

    // ── Profile 2: Cash Crunch (tight runway, needs cost cuts) ───────
    private async Task<IActionResult> SeedCashCrunch(int seed, CancellationToken ct)
    {
        var org = Organization.Create("ByteStack", $"cus_{seed}_byte", Money.From(28000, "USD"));
        await _orgRepo.AddAsync(org, ct);
        var orgId = org.Id;
        var random = new Random(seed);
        var now = DateTime.UtcNow;

        var transactions = new List<Transaction>();

        // Revenue: flat/declining ~$5-6K/mo
        var monthlySubBase = new[] { 6200m, 5800m, 5400m };
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var targetRevenue = monthlySubBase[2 - month];
            var subCount = 10 + random.Next(0, 4);
            var avgAmount = targetRevenue / subCount;

            for (int i = 0; i < subCount; i++)
            {
                var amount = avgAmount * (decimal)(0.7 + random.NextDouble() * 0.6);
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 22), random.Next(0, 60), 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRevenue(
                    orgId, Money.From(Math.Round(amount, 2), "USD"),
                    $"Subscription - Plan {(amount > 80 ? "Pro" : "Free→Starter")}",
                    $"evt_sub_{month}_{i}", $"pi_{Guid.NewGuid():N[..12]}", date));
            }

            // More refunds — churn is higher
            var refundCount = random.Next(1, 3);
            for (int i = 0; i < refundCount; i++)
            {
                var day = random.Next(1, 25);
                var date = new DateTime(monthStart.Year, monthStart.Month, day, 14, 0, 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRefund(orgId, Money.From(49 + random.Next(0, 50), "USD"),
                    "Refund - Customer churn (competitor switch)", null, date));
            }
        }

        // Expenses: ~$9K/mo — too high for revenue
        var expenses = new (ExpenseCategory, string, decimal)[]
        {
            (ExpenseCategory.Infrastructure, "AWS Cloud Hosting", 2800),
            (ExpenseCategory.Infrastructure, "Datadog Pro", 600),
            (ExpenseCategory.Infrastructure, "MongoDB Atlas", 450),
            (ExpenseCategory.Tools, "GitHub Enterprise", 1200),
            (ExpenseCategory.Tools, "Slack Business+", 350),
            (ExpenseCategory.Tools, "Jira Premium", 300),
            (ExpenseCategory.Marketing, "Google Ads", 1800),
            (ExpenseCategory.Marketing, "LinkedIn Ads", 900),
            (ExpenseCategory.Contractors, "QA Contractor", 2000),
            (ExpenseCategory.Office, "Coworking Space", 1200),
        };
        SeedExpenses(orgId, transactions, expenses, random, now);

        foreach (var tx in transactions)
            await _transactionRepo.AddAsync(tx, ct);

        var budgets = new[]
        {
            Budget.Create(orgId, ExpenseCategory.Infrastructure, Money.From(3500, "USD"), true, 85),
            Budget.Create(orgId, ExpenseCategory.Marketing, Money.From(2500, "USD"), false, 80),
            Budget.Create(orgId, ExpenseCategory.Tools, Money.From(1500, "USD"), false, 85),
            Budget.Create(orgId, ExpenseCategory.Contractors, Money.From(3000, "USD"), false, 70),
            Budget.Create(orgId, ExpenseCategory.Office, Money.From(1500, "USD"), true, 90),
        };
        await SeedBudgets(budgets, transactions, now, ct);

        // Agent decisions — urgent story
        var decisions = new[]
        {
            AgentDecision.Propose(orgId, AgentDecisionType.AlertRaised,
                "URGENT: Runway critically low — 3.1 months",
                "Current burn rate: $9,200/mo. Current revenue: $5,400/mo. Net burn: $3,800/mo. " +
                "Cash remaining: $28,000. At current trajectory, runway ends October 2026. " +
                "ALERT: Revenue declining 6% MoM while expenses remain flat. Immediate action required."),
            AgentDecision.Propose(orgId, AgentDecisionType.AnomalyDetected,
                "Revenue declining — 3 consecutive months of churn",
                "MRR dropped from $6,200 to $5,400 over 90 days. Churn rate: 8.2% (industry avg: 5%). " +
                "Primary churn reason: competitors offering lower pricing. " +
                "Recommend: Review pricing strategy, implement retention program, or pivot positioning."),
            AgentDecision.Propose(orgId, AgentDecisionType.ExpenseDenied,
                "QA contractor renewal denied — budget breach",
                "Evaluating expense request: $2,000/mo QA contractor renewal. Contractor budget remaining: $1,000. " +
                "Would exceed hard limit. Current burn rate already exceeds revenue. " +
                "DENY — recommend reducing contractor scope to $1,000/mo or bringing QA in-house."),
            AgentDecision.Propose(orgId, AgentDecisionType.BudgetAdjusted,
                "Recommended: cut infrastructure spend by 30%",
                "Infrastructure spend: $3,850/mo (highest category). Recommendations: " +
                "1) Downgrade AWS instances — save ~$800/mo. " +
                "2) Switch Datadog to free tier — save ~$600/mo. " +
                "3) Move MongoDB to self-hosted — save ~$450/mo. " +
                "Total potential savings: $1,850/mo. Would extend runway from 3.1 to 5.8 months."),
        };
        await SeedDecisions(decisions, ct);

        return Ok(new
        {
            OrganizationId = orgId,
            Profile = "cash-crunch",
            OrganizationName = "ByteStack",
            TransactionCount = transactions.Count,
            BudgetCount = budgets.Length,
            DecisionCount = decisions.Length,
            Message = "Cash Crunch profile seeded — ByteStack, $5.4K MRR, 3.1 months runway"
        });
    }

    // ── Profile 3: Hypergrowth (fast revenue, high burn) ─────────────
    private async Task<IActionResult> SeedHypergrowth(int seed, CancellationToken ct)
    {
        var org = Organization.Create("LaunchPad AI", $"cus_{seed}_launch", Money.From(180000, "USD"));
        await _orgRepo.AddAsync(org, ct);
        var orgId = org.Id;
        var random = new Random(seed);
        var now = DateTime.UtcNow;

        var transactions = new List<Transaction>();

        // Revenue: fast growth $35K → $55K over 3 months
        var monthlySubBase = new[] { 35000m, 43000m, 52000m };
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var targetRevenue = monthlySubBase[2 - month];
            var subCount = 30 + random.Next(0, 15);
            var avgAmount = targetRevenue / subCount;

            for (int i = 0; i < subCount; i++)
            {
                var amount = avgAmount * (decimal)(0.5 + random.NextDouble() * 1.0);
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 22), random.Next(0, 60), 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRevenue(
                    orgId, Money.From(Math.Round(amount, 2), "USD"),
                    $"Subscription - Plan {(amount > 200 ? "Enterprise" : amount > 100 ? "Team" : "Starter")}",
                    $"evt_sub_{month}_{i}", $"pi_{Guid.NewGuid():N[..12]}", date));
            }
        }

        // Expenses: ~$42K/mo — high but revenue is higher
        var expenses = new (ExpenseCategory, string, decimal)[]
        {
            (ExpenseCategory.Infrastructure, "AWS (GPU instances)", 12000),
            (ExpenseCategory.Infrastructure, "OpenAI API", 4000),
            (ExpenseCategory.Infrastructure, "Vercel + Cloudflare", 800),
            (ExpenseCategory.Tools, "GitHub Enterprise", 2000),
            (ExpenseCategory.Tools, "Linear Pro", 500),
            (ExpenseCategory.Tools, "Notion Enterprise", 400),
            (ExpenseCategory.Tools, "Figma Organization", 600),
            (ExpenseCategory.Marketing, "Google Ads", 5000),
            (ExpenseCategory.Marketing, "LinkedIn Ads", 3000),
            (ExpenseCategory.Marketing, "Conference Sponsorships", 4000),
            (ExpenseCategory.Contractors, "ML Contractor", 8000),
            (ExpenseCategory.Contractors, "Design Agency", 3000),
            (ExpenseCategory.Office, "Private Office Lease", 4500),
            (ExpenseCategory.Legal, "IP Filing", 2000),
        };
        SeedExpenses(orgId, transactions, expenses, random, now);

        foreach (var tx in transactions)
            await _transactionRepo.AddAsync(tx, ct);

        var budgets = new[]
        {
            Budget.Create(orgId, ExpenseCategory.Infrastructure, Money.From(18000, "USD"), true, 80),
            Budget.Create(orgId, ExpenseCategory.Marketing, Money.From(15000, "USD"), false, 75),
            Budget.Create(orgId, ExpenseCategory.Tools, Money.From(5000, "USD"), false, 70),
            Budget.Create(orgId, ExpenseCategory.Contractors, Money.From(12000, "USD"), false, 75),
            Budget.Create(orgId, ExpenseCategory.Office, Money.From(5000, "USD"), true, 85),
        };
        await SeedBudgets(budgets, transactions, now, ct);

        // Agent decisions — hypergrowth tension
        var decisions = new[]
        {
            AgentDecision.Propose(orgId, AgentDecisionType.AnomalyDetected,
                "GPU infrastructure costs surging — +38% in 30 days",
                "AWS GPU instance costs grew from $8,700 to $12,000. Driven by increased model training workloads. " +
                "Cost per training run: $420 (up from $310). " +
                "Recommend: Evaluate spot instances for non-critical training — potential 40% savings. " +
                "If trend continues, annual infrastructure cost: $144K."),
            AgentDecision.Propose(orgId, AgentDecisionType.AlertRaised,
                "Burn rate warning: 5.8 months runway at current spend",
                "Cash balance: $180,000. Monthly burn: $42,000. Monthly revenue: $52,000. " +
                "Net positive: $10K/mo — but committed hiring (5 engineers) would push burn to $82K/mo. " +
                "With hiring: runway drops to 3.2 months. Recommend closing Series B before expanding team."),
            AgentDecision.Propose(orgId, AgentDecisionType.ExpenseApproved,
                "Conference sponsorship approved — strategic",
                "Evaluating expense request: $4,000 AI conference sponsorship. Marketing budget remaining: $7,000. " +
                "Previous conference generated 12 enterprise leads (3 converted, $18K ARR). " +
                "Projected ROI: 4.5x. APPROVED — high-intent enterprise pipeline."),
            AgentDecision.Propose(orgId, AgentDecisionType.ReportGenerated,
                "Monthly summary — 22% MoM growth, burn needs attention",
                "MRR $52,000 (+22% MoM), Total expenses $42,000, Net income $10,000. " +
                "LTV $4,800, CAC payback 2.1 months, Gross margin 78%. " +
                "Revenue trajectory excellent. Key risk: burn rate growing faster than revenue. " +
                "Recommend: Raise Series B within 60 days or implement hiring freeze."),
        };
        await SeedDecisions(decisions, ct);

        return Ok(new
        {
            OrganizationId = orgId,
            Profile = "hypergrowth",
            OrganizationName = "LaunchPad AI",
            TransactionCount = transactions.Count,
            BudgetCount = budgets.Length,
            DecisionCount = decisions.Length,
            Message = "Hypergrowth profile seeded — LaunchPad AI, $52K MRR, 5.8 months runway"
        });
    }

    // ── Profile 4: Pre-Seed (solo founder, bootstrapped) ─────────────
    private async Task<IActionResult> SeedPreSeed(int seed, CancellationToken ct)
    {
        var org = Organization.Create("FreshStack", $"cus_{seed}_fresh", Money.From(35000, "USD"));
        await _orgRepo.AddAsync(org, ct);
        var orgId = org.Id;
        var random = new Random(seed);
        var now = DateTime.UtcNow;

        var transactions = new List<Transaction>();

        // Revenue: tiny but growing $400 → $1,200 over 3 months
        var monthlySubBase = new[] { 400m, 750m, 1100m };
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
            var targetRevenue = monthlySubBase[2 - month];
            var subCount = 3 + random.Next(0, 4);
            var avgAmount = targetRevenue / Math.Max(subCount, 1);

            for (int i = 0; i < subCount; i++)
            {
                var amount = avgAmount * (decimal)(0.7 + random.NextDouble() * 0.6);
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 22), random.Next(0, 60), 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateRevenue(
                    orgId, Money.From(Math.Round(amount, 2), "USD"),
                    $"Subscription - Early Adopter {(amount > 50 ? "Pro" : "Free")}",
                    $"evt_sub_{month}_{i}", $"pi_{Guid.NewGuid():N[..12]}", date));
            }
        }

        // Expenses: ~$3.5K/mo — lean
        var expenses = new (ExpenseCategory, string, decimal)[]
        {
            (ExpenseCategory.Infrastructure, "Vercel Pro", 200),
            (ExpenseCategory.Infrastructure, "Supabase Pro", 25),
            (ExpenseCategory.Tools, "GitHub Pro", 4),
            (ExpenseCategory.Tools, "Linear Free", 0),
            (ExpenseCategory.Tools, "Figma Starter", 15),
            (ExpenseCategory.Marketing, "Twitter Ads", 500),
            (ExpenseCategory.Marketing, "Indie Hackers Sponsor", 200),
            (ExpenseCategory.Office, "Home Office (stipend)", 150),
            (ExpenseCategory.Legal, "Terms of Service Draft", 800),
            (ExpenseCategory.Subscription, "Stripe fees (estimated)", 100),
        };
        SeedExpenses(orgId, transactions, expenses, random, now);

        foreach (var tx in transactions)
            await _transactionRepo.AddAsync(tx, ct);

        var budgets = new[]
        {
            Budget.Create(orgId, ExpenseCategory.Infrastructure, Money.From(500, "USD"), true, 80),
            Budget.Create(orgId, ExpenseCategory.Marketing, Money.From(1000, "USD"), false, 75),
            Budget.Create(orgId, ExpenseCategory.Tools, Money.From(100, "USD"), false, 80),
            Budget.Create(orgId, ExpenseCategory.Office, Money.From(200, "USD"), false, 70),
            Budget.Create(orgId, ExpenseCategory.Legal, Money.From(1000, "USD"), true, 90),
        };
        await SeedBudgets(budgets, transactions, now, ct);

        // Agent decisions — solo founder guidance
        var decisions = new[]
        {
            AgentDecision.Propose(orgId, AgentDecisionType.ForecastUpdated,
                "Runway: 8.2 months — monitor closely",
                "Cash balance: $35,000. Monthly burn: $3,200. Monthly revenue: $1,100. " +
                "Net burn: $2,100/mo. Runway: 8.2 months (ends Feb 2027). " +
                "Revenue growing 45% MoM — if sustained, breakeven in ~4 months. " +
                "Key milestone: reach $3,200 MRR to become cash-flow positive."),
            AgentDecision.Propose(orgId, AgentDecisionType.AnomalyDetected,
                "Twitter ads CAC increasing — review channel",
                "Twitter ads spend: $500/mo, generating 8 signups (2 paid conversions). " +
                "CAC: $250, but LTV only $180 at current pricing. Negative unit economics. " +
                "Recommend: Pause Twitter ads, focus on organic content + Indie Hackers community."),
            AgentDecision.Propose(orgId, AgentDecisionType.ExpenseApproved,
                "Legal ToS draft approved — necessary investment",
                "Evaluating expense request: $800 one-time for terms of service drafting. " +
                "Legal budget available: $1,000. One-time expense, not recurring. " +
                "Required before accepting paid customers. APPROVED — foundational expense."),
            AgentDecision.Propose(orgId, AgentDecisionType.ReportGenerated,
                "Week 12 summary — first paying customers milestone",
                "MRR $1,100 (+47% MoM), 7 paying customers, 42 free users. " +
                "Biggest win: 3 customers upgraded from free after feature request. " +
                "Biggest risk: Solo founder burnout — no backup if founder is unavailable. " +
                "Next priority: Reach $3K MRR before considering first hire."),
        };
        await SeedDecisions(decisions, ct);

        return Ok(new
        {
            OrganizationId = orgId,
            Profile = "pre-seed",
            OrganizationName = "FreshStack",
            TransactionCount = transactions.Count,
            BudgetCount = budgets.Length,
            DecisionCount = decisions.Length,
            Message = "Pre-Seed profile seeded — FreshStack, $1.1K MRR, solo founder"
        });
    }

    // ── Shared helpers ────────────────────────────────────────────────

    private void SeedExpenses(
        Guid orgId, List<Transaction> transactions,
        (ExpenseCategory, string, decimal)[] expenseDefs,
        Random random, DateTime now)
    {
        for (int month = 2; month >= 0; month--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-month);
            var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);

            foreach (var (category, desc, baseAmount) in expenseDefs)
            {
                if (baseAmount == 0) continue;
                var amount = baseAmount * (1 + (decimal)(random.NextDouble() * 0.1 - 0.05));
                var day = random.Next(1, Math.Min(daysInMonth, 28));
                var date = new DateTime(monthStart.Year, monthStart.Month, day,
                    random.Next(8, 18), random.Next(0, 60), 0, DateTimeKind.Utc);
                transactions.Add(Transaction.CreateExpense(
                    orgId, Money.From(Math.Round(amount, 2), "USD"), desc, category, null, date));
            }
        }
    }

    private async Task SeedBudgets(Budget[] budgets, List<Transaction> transactions, DateTime now, CancellationToken ct)
    {
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
    }

    private async Task SeedDecisions(AgentDecision[] decisions, CancellationToken ct)
    {
        foreach (var decision in decisions)
            await _decisionRepo.AddAsync(decision, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
