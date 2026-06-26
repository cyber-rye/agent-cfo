using AgentCfo.Core.Common;

namespace AgentCfo.Core.Entities;

public class Organization : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string StripeCustomerId { get; private set; } = string.Empty;
    public string? StripeAccountId { get; private set; }
    public Money MonthlyBudget { get; private set; } = Money.Zero();
    public int RunwayThresholdDays { get; private set; } = 90;
    
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    
    private readonly List<Budget> _budgets = new();
    public IReadOnlyCollection<Budget> Budgets => _budgets.AsReadOnly();
    
    private readonly List<Forecast> _forecasts = new();
    public IReadOnlyCollection<Forecast> Forecasts => _forecasts.AsReadOnly();
    
    private readonly List<AgentDecision> _decisions = new();
    public IReadOnlyCollection<AgentDecision> Decisions => _decisions.AsReadOnly();
    
    private readonly List<AuditEntry> _auditEntries = new();
    public IReadOnlyCollection<AuditEntry> AuditEntries => _auditEntries.AsReadOnly();
    
    private Organization() { }
    
    public static Organization Create(string name, string stripeCustomerId, Money monthlyBudget, int runwayThresholdDays = 90)
    {
        return new Organization
        {
            Name = name,
            StripeCustomerId = stripeCustomerId,
            MonthlyBudget = monthlyBudget,
            RunwayThresholdDays = runwayThresholdDays
        };
    }
    
    public void UpdateStripeAccountId(string accountId)
    {
        StripeAccountId = accountId;
        MarkUpdated();
    }
    
    public void UpdateBudget(Money monthlyBudget)
    {
        MonthlyBudget = monthlyBudget;
        MarkUpdated();
    }
    
    public void SetRunwayThreshold(int days)
    {
        RunwayThresholdDays = days;
        MarkUpdated();
    }
}
