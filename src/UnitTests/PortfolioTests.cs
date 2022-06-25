using Domain;
using FluentAssertions;

namespace UnitTests;

public class PortfolioTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Portfolio_CreateNewPortfolio_ReturnsValidPortfolio()
    {
        var port = Portfolio.CreateNew();
        port.Id.Should().NotBeNullOrWhiteSpace();
        port.PendingEvents.Count.Should().Be(1);
    }
}