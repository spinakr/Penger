
namespace Domain.ValueObjects;

public class Investment
{
    public InvestmentType InvestmentType { get; set; }
    public InvestmentGroup InvestmentGroup { get; set; }
    public string Name { get; set; }

    public Investment(string name, InvestmentType type, InvestmentGroup group)
    {
        InvestmentType = type;
        Name = name;
        InvestmentGroup = group;
    }
}