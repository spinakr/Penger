using System.Globalization;
using Newtonsoft.Json;

namespace Domain.ValueObjects;

public class NokMoney : Money
{
    public NokMoney(decimal value) : base(value, CurrencyType.NOK)
    {
    }

}

public class Money : ValueObject, IComparable<Money>
{
    public decimal Value { get; private set; }
    public CurrencyType Currency { get; private set; }
    public static Money ZERO => new Money(0, CurrencyType.NA);

    public Money(int value, CurrencyType currency) : this(new decimal(value), currency) { }

    public Money(decimal value, string currency) : this(value, Enumeration.FromDisplayName<CurrencyType>(currency))
    {
    }

    [JsonConstructor]
    public Money(decimal value, CurrencyType currency)
    {
        // if (value < 0) throw new ArgumentException("Money cannot be negative");
        if (currency == null) throw new ArgumentException("Currency cannot be null");
        Value = value;
        Currency = currency;
    }

    public bool IsZero => Value == 0;

    public NokMoney ToNok(double toNokConversionRate)
    {
        return new NokMoney(Value * (decimal)toNokConversionRate);
    }


    public static Money operator *(Money money, Amount amount)
    {
        return new Money(money.Value * new decimal(amount.Value), money.Currency);
    }

    public static Money operator +(Money m1, Money m2)
    {
        if (m1.Currency != m2.Currency) throw new ArgumentException("Cannot add different currencies");
        return new Money(m1.Value + m2.Value, m1.Currency);
    }

    public static Money operator -(Money m1, Money m2)
    {
        if (m1.Currency != m2.Currency) throw new ArgumentException("Cannot subtract different currencies");
        return new Money(m1.Value - m2.Value, m1.Currency);
    }

    public Percent PercentOf(Money m2)
    {
        if (m2.Value == 0) throw new ArgumentException("Cannot divide by zero");
        return new Percent(Value / m2.Value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
        yield return Currency;
    }

    public override string ToString()
    {
        if (Currency == CurrencyType.NOK)
        {
            return Value.ToString("C", new CultureInfo("no-NO"));
        }
        else
        {
            return $"{Math.Round(Value, 2)} {Currency.ToString()}";
        }
    }

    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        if (Currency != other.Currency) throw new ArgumentException("Cannot compare different currencies");
        return Value.CompareTo(other.Value);
    }
}