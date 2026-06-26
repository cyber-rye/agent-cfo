using AgentCfo.Core.Common;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Entities;

public class Transaction : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string? StripeEventId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; } = Money.Zero();
    public string Description { get; private set; } = string.Empty;
    public ExpenseCategory Category { get; private set; } = ExpenseCategory.Other;
    public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;
    public string? StripePaymentIntentId { get; private set; }
    public string? StripeInvoiceId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Metadata { get; private set; }
    
    public Organization Organization { get; private set; } = null!;
    
    private Transaction() { }
    
    public static Transaction CreateRevenue(Guid organizationId, Money amount, string description, string? stripeEventId = null, string? stripePaymentIntentId = null)
    {
        return new Transaction
        {
            OrganizationId = organizationId,
            Type = TransactionType.Revenue,
            Amount = amount,
            Description = description,
            Category = ExpenseCategory.Subscription,
            Status = TransactionStatus.Completed,
            StripeEventId = stripeEventId,
            StripePaymentIntentId = stripePaymentIntentId,
            OccurredAt = DateTime.UtcNow
        };
    }
    
    public static Transaction CreateExpense(Guid organizationId, Money amount, string description, ExpenseCategory category, string? stripeEventId = null)
    {
        return new Transaction
        {
            OrganizationId = organizationId,
            Type = TransactionType.Expense,
            Amount = amount,
            Description = description,
            Category = category,
            Status = TransactionStatus.Completed,
            StripeEventId = stripeEventId,
            OccurredAt = DateTime.UtcNow
        };
    }
    
    public static Transaction CreateRefund(Guid organizationId, Money amount, string description, string? stripePaymentIntentId = null)
    {
        return new Transaction
        {
            OrganizationId = organizationId,
            Type = TransactionType.Refund,
            Amount = amount,
            Description = description,
            Status = TransactionStatus.Refunded,
            StripePaymentIntentId = stripePaymentIntentId,
            OccurredAt = DateTime.UtcNow
        };
    }
    
    public void Categorize(ExpenseCategory category)
    {
        Category = category;
        MarkUpdated();
    }
    
    public void UpdateStatus(TransactionStatus status)
    {
        Status = status;
        MarkUpdated();
    }
}
