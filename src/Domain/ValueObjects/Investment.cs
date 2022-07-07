
namespace Domain.ValueObjects;

public class Investment : ValueObject
{
    public InvestmentId Id { get; private set; }
    public InvestmentType Type { get; private set; }
    public InvestmentGroup Group { get; private set; }
    public string Symbol { get; private set; }
    public string Currency { get; private set; }

    public Investment(InvestmentId id, InvestmentType type, InvestmentGroup group, string symbol, string currency)
    {
        Id = id;
        Type = type;
        Group = group;
        Symbol = symbol;
        Currency = currency;
    }

    public Investment(string id, string type, string group, string symbol, string currency)
    {
        Id = new InvestmentId(id);
        Type = Enumeration.FromDisplayName<InvestmentType>(type);
        Group = Enumeration.FromDisplayName<InvestmentGroup>(group);
        Symbol = symbol;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Type;
        yield return Group;
    }

}

