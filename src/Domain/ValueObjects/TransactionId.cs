
namespace Domain.ValueObjects;

public class TransactionId : ValueObject
{
    public readonly Guid Value;

    public TransactionId()
    {
        Value = Guid.NewGuid();
    }
    public TransactionId(Guid value)
    {
        if (value == Guid.Empty) throw new InvalidDataException("TransactionId can not be null");
        Value = value;
    }
    public TransactionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidDataException("TransactionId can not be null");
        Value = Guid.Parse(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}