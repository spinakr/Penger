using Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PocketCqrs;
using PocketCqrs.EventStore;
using Web.Commands;

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

        _messaging.Dispatch(new Web.Pages.Investments.Create.Command
        {
            PortfolioId = id,
            InvestmentId = "Gold",
            InvestmentGroup = InvestmentGroup.Gold.DisplayName,
            InvestmentType = InvestmentType.Commodity.DisplayName
        });

        _messaging.Dispatch(new AddTransactionCommand
        {
            PortfolioId = id,
            Date = DateTime.Now,
            InvestmentId = "Gold",
            Amount = 10,
            Price = new decimal(10.5),
            Fee = 5,
            Currency = "NOK"
        });

        var reloadedPort = _messaging.Dispatch(new Web.Pages.Transactions.Index.Query
        {
            PortfolioId = id
        });

        reloadedPort.Transactions.Count.Should().Be(1);
    }
}