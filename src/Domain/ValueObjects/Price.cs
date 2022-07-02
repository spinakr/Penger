using Newtonsoft.Json;

namespace Domain.ValueObjects;

public class Price
{
    public decimal Value { get; private set; }
    public CurrencyType Currency { get; private set; }
    public static Price ZERO => new Price(0, CurrencyType.NA);

    public Price(decimal value, string currency) : this(value, Enumeration.FromDisplayName<CurrencyType>(currency))
    {
    }

    [JsonConstructor]
    public Price(decimal value, CurrencyType currency)
    {
        if (value < 0) throw new ArgumentException("Price cannot be negative");
        if (currency == null) throw new ArgumentException("Currency cannot be null");
        Value = value;
        Currency = currency;
    }
}