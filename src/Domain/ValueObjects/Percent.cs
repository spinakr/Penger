namespace Domain.ValueObjects;

public class Percent : IComparable<Percent>
{
    private readonly double percent;
    public double Fraction => percent / 100;
    public double PercentageValue => percent;

    public Percent(double fraction)
    {
        IsTrue(fraction >= 0 && fraction <= 1, "Percentage value must be between 0 and 1");
        percent = fraction * 100;
    }

    public int CompareTo(Percent? other)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return $"{percent}%";
    }
}