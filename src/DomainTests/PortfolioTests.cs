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
}