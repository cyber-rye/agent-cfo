namespace AgentCfo.Core.Common;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    
    public static Money Zero(string currency = "USD") => new() { Amount = 0, Currency = currency };
    public static Money From(decimal amount, string currency = "USD") => new() { Amount = amount, Currency = currency };
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");
        return this with { Amount = Amount + other.Amount };
    }
    
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency) throw new InvalidOperationException($"Cannot subtract {other.Currency} from {Currency}");
        return this with { Amount = Amount - other.Amount };
    }
    
    public Money Multiply(decimal factor) => this with { Amount = Amount * factor };
    
    public bool IsGreaterThan(Money other) => Amount > other.Amount;
    public bool IsLessThan(Money other) => Amount < other.Amount;
    public bool IsZero => Amount == 0;
    public bool IsNegative => Amount < 0;
    
    public override string ToString() => $"{Amount:N2} {Currency}";
}
