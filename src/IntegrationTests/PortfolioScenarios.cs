using Domain;
using Domain.ValueObjects;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PocketCqrs;
using PocketCqrs.EventStore;

namespace IntegrationTests;

public class PortfolioScenarios
{
    private IMessaging _messaging;

    [SetUp]
    public void Setup()
    {
        var provider = new ServiceCollection()
            .AddHandlers(typeof(CreatePortfolioCommandHandler).Assembly)
            .AddSingleton<IMessaging, Messaging>()
            .AddSingleton<IEventStore, EventStore>()
            .AddSingleton<IAppendOnlyStore, InMemoryAppendOnlyStore>()
            .BuildServiceProvider();


        _messaging = provider.GetService<IMessaging>();
    }

    [Test]
    public void CreatNewPortfolio()
    {

        var result = (Result<string>)_messaging.Dispatch(new CreatePortfolioCommand());
        var id = result.Value;

        _messaging.Dispatch(new AddTransactionCommand
        {
            PortfolioId = id,
            Date = DateTime.Now,
            InvestmentName = "Gold",
            Amount = 10,
            Price = new decimal(10.5),
            Fee = 5,
            Currency = "NOK"
        });

        var reloadedPort = _messaging.Dispatch(new GetPortfolioAggregateQuery
        {
            PortfolioId = id
        });

        reloadedPort.Transactions.Count.Should().Be(1);
        reloadedPort.Id.Should().Be(id);
    }
}