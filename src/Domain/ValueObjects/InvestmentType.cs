namespace Domain.ValueObjects;

public class InvestmentType : Enumeration
{
    public static readonly InvestmentType Fund = new InvestmentType(0, "Fund");
    public static readonly InvestmentType Cryptocurrency = new InvestmentType(1, "Cryptocurrency");
    public static readonly InvestmentType ETF = new InvestmentType(2, "ETF");
    public static readonly InvestmentType Stock = new InvestmentType(3, "Stock");

    private InvestmentType() { }
    private InvestmentType(int value, string displayName) : base(value, displayName) { }
}