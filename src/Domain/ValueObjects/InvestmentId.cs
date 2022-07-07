
namespace Domain.ValueObjects;

public class InvestmentId : ValueObject
{
    public readonly string Value;

    public InvestmentId(string value)
    {
        Value = NotNullOrEmpty(value);
    }

    public static explicit operator InvestmentId(string value)
    {
        return new InvestmentId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}