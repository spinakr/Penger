namespace Domain.ValueObjects;

public class InvestmentGroup : Enumeration
{
    public static readonly InvestmentGroup TechETF = new InvestmentGroup(0, "Technology ETF");
    public static readonly InvestmentGroup Cryptocurrency = new InvestmentGroup(1, "Cryptocurrency");
    public static readonly InvestmentGroup GlobalIndex = new InvestmentGroup(2, "Global Index Funds");
    public static readonly InvestmentGroup EmergingMarkets = new InvestmentGroup(3, "Emerging Markets Funds");
    public static readonly InvestmentGroup SingleStocks = new InvestmentGroup(4, "Single Stocks");
    public static readonly InvestmentGroup Gold = new InvestmentGroup(5, "Gold");
    public InvestmentGroup() { }
    private InvestmentGroup(int value, string displayName) : base(value, displayName) { }
}
