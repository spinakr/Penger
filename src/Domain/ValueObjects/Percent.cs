namespace Domain.ValueObjects;

public class Percent : IComparable<Percent>
{
    private readonly double percent;
    public double Fraction => percent / 100;
    public double PercentageValue => percent;

    public Percent(double fraction)
    {
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