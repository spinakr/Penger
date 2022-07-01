namespace Domain.ValueObjects;

public class Transaction
{
    public DateTime Date { get; private set; }
    public TransactionId TransactionId { get; private set; }
    public InvestmentId InvestmentId { get; set; }
    public TransactionType Type { get; set; }


    public double Amount { get; private set; }
    public Price Price { get; private set; }
    public Price Fee { get; private set; }

    public Transaction(InvestmentId investmentId, TransactionId transactionId, DateTime date, double amount, Price price, Price fee, TransactionType? type = null)
    {
        TransactionId = transactionId;
        InvestmentId = investmentId;
        Type = type ?? TransactionType.Purchase;
        Date = date;
        Amount = amount;
        Price = price;
        Fee = fee;
    }

    public static Transaction CreateNew(InvestmentId investmentId, DateTime date, double amount, Price price, Price fee, TransactionType? type = null)
    {
        //TODO: transaction rules
        return new Transaction(investmentId, new TransactionId(), date, amount, price, fee, type);
    }
}