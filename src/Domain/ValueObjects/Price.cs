using Newtonsoft.Json;

namespace Domain.ValueObjects;

public class NokPrice : Price
{
    public NokPrice(decimal value) : base(value, CurrencyType.NOK)
    {
    }

}

public class Price
{
    public decimal Value { get; private set; }
    public CurrencyType Currency { get; private set; }
    public static Price ZERO => new Price(0, CurrencyType.NA);

    public Price(decimal value, string currency) : this(value, Enumeration.FromDisplayName<CurrencyType>(currency))
    {
    }

    public NokPrice ToNok(double toNokConversionRate)
    {
        return new NokPrice(Value * (decimal)toNokConversionRate);
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