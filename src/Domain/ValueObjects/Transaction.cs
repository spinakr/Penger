namespace Domain.ValueObjects;

public class Transaction
{
    public DateTime Date { get; private set; }
    public TransactionId TransactionId { get; private set; }
    public InvestmentId InvestmentId { get; set; }
    public double Amount { get; private set; }
    public decimal Price { get; private set; }
    public decimal Fee { get; private set; }
    public string Currency { get; private set; }

    public Transaction(InvestmentId investmentId, TransactionId transactionId, DateTime date, double amount, decimal price, decimal fee, string currency)
    {
        TransactionId = transactionId;
        InvestmentId = investmentId;
        Date = date;
        Amount = amount;
        Price = price;
        Fee = fee;
        Currency = currency;
    }

    public static Transaction CreateNew(InvestmentId investmentId, DateTime date, double amount, decimal price, decimal fee, string currency)
    {
        //TODO: transaction rules
        return new Transaction(investmentId, new TransactionId(), date, amount, price, fee, currency);
    }
}