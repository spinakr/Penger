
namespace Domain.ValueObjects;

public class Investment : ValueObject
{
    public InvestmentId Id { get; private set; }
    public InvestmentType Type { get; private set; }
    public InvestmentGroup Group { get; private set; }

    public Investment(InvestmentId id, InvestmentType type, InvestmentGroup group)
    {
        Id = id;
        Type = type;
        Group = group;
    }
    public Investment(string id, string type, string group)
    {
        Id = new InvestmentId(id);
        Type = Enumeration.FromDisplayName<InvestmentType>(type);
        Group = Enumeration.FromDisplayName<InvestmentGroup>(group);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Type;
        yield return Group;
    }

}

