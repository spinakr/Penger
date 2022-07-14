using Domain;
using Domain.Events;
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


    [Test]
    public void RegisterInvestment_Duplicate_ShouldThrow()
    {
        var port = Portfolio.CreateNew("TEST");

        port.RegisterInvestment(new Investment(
            id: new InvestmentId("1"),
            type: InvestmentType.Stock,
            group: InvestmentGroup.GlobalIndex,
            symbol: new Symbol("AAPL"),
            currency: CurrencyType.NOK));

        ((Action)(() =>

        port.RegisterInvestment(new Investment(
            id: new InvestmentId("1"),
            type: InvestmentType.Stock,
            group: InvestmentGroup.GlobalIndex,
            symbol: new Symbol("AAPL"),
            currency: CurrencyType.NOK))

        )).Should().Throw<InvalidDataException>();
    }

    [Test]
    public void UpdatePrice_KnownInvestment_ShouldUpdatePrice()
    {
        var port = Portfolio.CreateNew("TEST");

        port.RegisterInvestment(new Investment(
            id: new InvestmentId("1"),
            type: InvestmentType.Stock,
            group: InvestmentGroup.GlobalIndex,
            symbol: new Symbol("AAPL"),
            currency: CurrencyType.NOK));


        port.UpdatePrice(new InvestmentId("1"), new NokMoney(100), new Money(100, CurrencyType.NOK));

        port.PendingEvents.Count.Should().Be(3);
        (port.PendingEvents.Last() as InvestmentPriceWasUpdated).InvestmentId.Should().Be("1");
    }

    [Test]
    public void UpdatePrice_UnknownInvestment_ShouldThrow()
    {
        var port = Portfolio.CreateNew("TEST");

        ((Action)(() =>

        port.UpdatePrice(new InvestmentId("1"), new NokMoney(100), new Money(100, CurrencyType.NOK))

        )).Should().Throw<InvalidDataException>();
    }


}