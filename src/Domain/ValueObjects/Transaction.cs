namespace Domain.ValueObjects;

public class Transaction
{
    public DateTime Date { get; private set; }
    public InvestmentId InvestmentId { get; private set; }
    public int Amount { get; private set; }
    public decimal Price { get; private set; }
    public decimal Fee { get; private set; }
    public string Currency { get; private set; }

    public Transaction(InvestmentId investmentId, DateTime date, int amount, decimal price, decimal fee, string currency)
    {
        InvestmentId = investmentId;
        Date = date;
        Amount = amount;
        Price = price;
        Fee = fee;
        Currency = currency;
    }

    public static Transaction CreateNew(InvestmentId investmentId, DateTime date, int amount, decimal price, decimal fee, string currency)
    {
        //TODO: transaction rules
        return new Transaction(investmentId, date, amount, price, fee, currency);
    }
}