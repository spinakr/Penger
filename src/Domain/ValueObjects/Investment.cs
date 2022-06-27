
namespace Domain.ValueObjects;

public class Investment
{
    public string Name { get; private set; }
    public InvestmentType Type { get; private set; }

    public Investment(string name, InvestmentType type)
    {
        Name = name;
        Type = type;
    }
}

