namespace Domain.ValueObjects;

public class Transaction
{
    public DateTime Date { get; private set; }
    public TransactionId TransactionId { get; private set; }
    public InvestmentId InvestmentId { get; set; }
    public TransactionType Type { get; set; }


    public Amount Amount { get; private set; }
    public Money Price { get; private set; }
    public Money Fee { get; private set; }

    public Transaction(InvestmentId investmentId, TransactionId transactionId, DateTime date,
        Amount amount, Money price, Money fee, TransactionType? type = null)
    {
        TransactionId = transactionId;
        InvestmentId = investmentId;
        Type = type ?? TransactionType.Purchase;
        Date = date;
        Amount = amount;
        Price = price;
        Fee = fee;
    }

    public static Transaction ParseFromCSVLine(string line)
    {
        //dato;navn;type;antall;pris;avgift;kurs;valuta
        var parts = line.Split(';');
        var date = DateTime.Parse(parts[0]);
        var investmentId = new InvestmentId(parts[1]);
        var type = parts[2] == "Salg" ? TransactionType.Sale : TransactionType.Purchase;
        var amount = double.Parse(string.Concat(parts[3].Where(s => !char.IsWhiteSpace(s) && s != '-')).Replace(',', '.')); //
        var conversionRate = string.IsNullOrWhiteSpace(parts[6]) ? 1 : decimal.Parse(string.Concat(parts[6].Where(c => !char.IsWhiteSpace(c))).Replace(',', '.')); //
        var price = new NokMoney(decimal.Parse(string.Concat(parts[4].Where(c => !char.IsWhiteSpace(c))).Replace(',', '.')) * conversionRate);
        var fee = string.IsNullOrWhiteSpace(parts[5]) ? null : new NokMoney(decimal.Parse(parts[5].Replace(',', '.')) * conversionRate);

        return new Transaction(investmentId, new TransactionId(), date, new Amount(amount), price, fee, type);
    }
}