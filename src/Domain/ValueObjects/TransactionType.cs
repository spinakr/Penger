namespace Domain.ValueObjects;

public class TransactionType : Enumeration
{
    public static readonly TransactionType Sale = new TransactionType(0, "Sale");
    public static readonly TransactionType Purchase = new TransactionType(1, "Purchase");

    public TransactionType() { }
    private TransactionType(int value, string displayName) : base(value, displayName) { }
}