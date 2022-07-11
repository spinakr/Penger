
namespace Domain.ValueObjects;

public class Investment : ValueObject
{
    public InvestmentId Id { get; private set; }
    public InvestmentType Type { get; private set; }
    public InvestmentGroup Group { get; private set; }
    public CurrencyType Currency { get; private set; }
    public Symbol Symbol { get; private set; }

    public Investment(InvestmentId id, InvestmentType type, InvestmentGroup group, Symbol symbol, CurrencyType currency)
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
        Currency = Enumeration.FromDisplayName<CurrencyType>(currency);
        Symbol = new Symbol(symbol);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Type;
        yield return Group;
    }

}

