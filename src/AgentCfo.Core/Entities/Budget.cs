using AgentCfo.Core.Common;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Entities;

public class Budget : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public ExpenseCategory Category { get; private set; }
    public Money MonthlyLimit { get; private set; } = Money.Zero();
    public Money CurrentSpend { get; private set; } = Money.Zero();
    public BudgetPeriod Period { get; private set; } = BudgetPeriod.Monthly;
    public int AlertThresholdPercent { get; private set; } = 80;
    public bool HardLimit { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    public Organization Organization { get; private set; } = null!;
    
    private Budget() { }
    
    public static Budget Create(Guid organizationId, ExpenseCategory category, Money monthlyLimit, bool hardLimit = false, int alertThresholdPercent = 80)
    {
        return new Budget
        {
            OrganizationId = organizationId,
            Category = category,
            MonthlyLimit = monthlyLimit,
            HardLimit = hardLimit,
            AlertThresholdPercent = alertThresholdPercent
        };
    }
    
    public decimal PercentUsed => MonthlyLimit.IsZero ? 0 : (CurrentSpend.Amount / MonthlyLimit.Amount) * 100;
    public bool IsOverBudget => CurrentSpend.IsGreaterThan(MonthlyLimit);
    public bool IsNearLimit => PercentUsed >= AlertThresholdPercent;
    public Money Remaining => MonthlyLimit.Subtract(CurrentSpend);
    
    public bool CanSpend(Money amount)
    {
        if (!HardLimit) return true;
        return !CurrentSpend.Add(amount).IsGreaterThan(MonthlyLimit);
    }
    
    public void RecordSpend(Money amount)
    {
        CurrentSpend = CurrentSpend.Add(amount);
        MarkUpdated();
    }
    
    public void ResetSpend()
    {
        CurrentSpend = Money.Zero(MonthlyLimit.Currency);
        MarkUpdated();
    }
    
    public void UpdateLimit(Money newLimit)
    {
        MonthlyLimit = newLimit;
        MarkUpdated();
    }
    
    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
