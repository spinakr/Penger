using Domain;
using Domain.ValueObjects;
using FluentAssertions;

namespace DomainTests;

public class PortfolioTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Portfolio_CreateNewPortfolio_ReturnsValidPortfolio()
    {
        var port = Portfolio.CreateNew("TEST");
        port.Id.Should().NotBeNullOrWhiteSpace();
        port.PendingEvents.Count.Should().Be(1);
    }

    [Test]
    public void ChangeDistribution_WithValidSetup_NotFail()
    {
        var port = Portfolio.CreateNew("TEST");
        port.ChangeDistribution(new Dictionary<InvestmentGroup, Percent>
        {
            { InvestmentGroup.GlobalIndex, new Percent(0.55) },
            { InvestmentGroup.EmergingMarkets, new Percent(0.15) },
            { InvestmentGroup.TechETF, new Percent(0.10) },
            { InvestmentGroup.Cryptocurrency, new Percent(0.10) },
            { InvestmentGroup.Gold, new Percent(0.05) },
            { InvestmentGroup.SingleStocks, new Percent(0.05) }
        });

        port.PendingEvents.Count.Should().Be(2);
    }

    [Test]
    public void ChangeDistribution_WithInValidSetup_NotFail()
    {
        var port = Portfolio.CreateNew("TEST");
        ((Action)(() =>
        port.ChangeDistribution(new Dictionary<InvestmentGroup, Percent>
        {
            { InvestmentGroup.GlobalIndex, new Percent(0.55) },
            { InvestmentGroup.TechETF, new Percent(0.10) },
            { InvestmentGroup.Cryptocurrency, new Percent(0.10) },
            { InvestmentGroup.Gold, new Percent(0.05) },
            { InvestmentGroup.SingleStocks, new Percent(0.05) }
        }))).Should().Throw<InvalidDataException>();
    }


    // [Test]
    // public void TotalPortfolioValue_SimpleBuyAndSell_ShouldCalculateCorrectly()
    // {
    //     var port = Portfolio.CreateNew("TEST");
    //     port.RegisterInvestment(new Investment(new InvestmentId("DNB Global Indeks"), InvestmentType.Fund, InvestmentGroup.GlobalIndex));
    //     port.RegisterInvestment(new Investment(new InvestmentId("KLP fremvoksende marked indeks 2"), InvestmentType.Fund, InvestmentGroup.GlobalIndex));
    //     port.RegisterInvestment(new Investment(new InvestmentId("Gold"), InvestmentType.Commodity, InvestmentGroup.Gold));

    //     port.AddTransaction(Transaction.CreateNew(new InvestmentId("DNB Global Indeks"), new DateTime(2022, 1, 1), 10, new Price(100, CurrencyType.NOK), Price.ZERO)); //1000 NOK
    //     port.AddTransaction(Transaction.CreateNew(new InvestmentId("KLP fremvoksende marked indeks 2"), new DateTime(2022, 1, 1), 9, new Price(200, CurrencyType.NOK), Price.ZERO)); // 1800 NOK
    //     port.AddTransaction(Transaction.CreateNew(new InvestmentId("Gold"), new DateTime(2022, 1, 1), 2, new Price(1000, CurrencyType.NOK), Price.ZERO)); // 2000 NOK
    //     port.AddTransaction(Transaction.CreateNew(new InvestmentId("DNB Global Indeks"), new DateTime(2022, 1, 1), 10, new Price(200, CurrencyType.NOK), Price.ZERO, TransactionType.Sale)); // -1000 NOK // SUM : 2900 NOK

    // }
}