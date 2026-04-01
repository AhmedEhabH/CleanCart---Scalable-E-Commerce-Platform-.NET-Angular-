namespace ECommerce.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        return new Money(Math.Round(amount, 2), currency);
    }

    public static Money Zero(string currency = "USD") => new(0, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        
        return new Money(Math.Max(0, Amount - other.Amount), Currency);
    }

    public Money Multiply(int quantity) => new(Amount * quantity, Currency);

    public override string ToString() => $"{Currency} {Amount:N2}";
}
