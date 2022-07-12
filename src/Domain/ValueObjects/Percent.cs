
using Newtonsoft.Json;

namespace Domain.ValueObjects;

public class Percent : IComparable<Percent>
{
    private readonly double fractionValue;
    public double Fraction => fractionValue;
    [JsonIgnore]
    public double PercentageValue => fractionValue * 100;

    public Percent(decimal fraction) : this((double)fraction)
    {
    }

    [JsonConstructor]
    public Percent(double fraction)
    {
        IsTrue(fraction >= 0 && fraction <= 1, "Percentage value must be between 0 and 1");
        fractionValue = fraction;
    }

    public int CompareTo(Percent? other)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return $"{Math.Round(PercentageValue, 1)}%";
    }
}