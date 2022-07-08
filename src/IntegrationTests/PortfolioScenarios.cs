using Domain.Projections;
using Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PocketCqrs;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;
using Web.Commands;

namespace IntegrationTests;

public class PortfolioScenarios
{
    private IMediator _messaging;

    [SetUp]
    public void Setup()
    {
        var provider = new ServiceCollection()
            .AddHandlers(typeof(Web.StartupWorker).Assembly)
            .AddMediatR(typeof(Web.StartupWorker).Assembly)
            .AddSingleton<IMessaging, Messaging>()
            .AddSingleton<IEventStore, EventStore>()
            .AddSingleton<IAppendOnlyStore, InMemoryAppendOnlyStore>()
            .AddSingleton<IProjectionStore<string, PortfolioStatus>, FileProjectionStore<string, PortfolioStatus>>()
            .BuildServiceProvider();


        _messaging = provider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task CreatNewPortfolio()
    {
        var id = await _messaging.Send(new CreatePortfolioCommand { Name = "Test" });

        await _messaging.Send(new Web.Pages.Investments.Create.Command
        {
            PortfolioId = id,
            InvestmentId = "Gold",
            InvestmentGroup = InvestmentGroup.Gold.DisplayName,
            InvestmentType = InvestmentType.Commodity.DisplayName
        });

        await _messaging.Send(new Web.Pages.Transactions.Create.Command
        {
            PortfolioId = id,
            Date = DateTime.Now,
            InvestmentId = "Gold",
            Amount = 10,
            Price = new decimal(10.5),
            Fee = 5,
            Currency = "NOK"
        });

        var reloadedPort = await _messaging.Send(new Web.Pages.Transactions.Index.Query
        {
            PortfolioId = id
        });

        reloadedPort.Transactions.Count.Should().Be(1);
    }
}