using System.Globalization;
using Domain;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using PocketCqrs.Projections;

namespace Web.Projections;

public class PortfolioStatusProjection : INotificationHandler<NewTransactionWasCreated>
{
    public class PortfolioStatus
    {
        public string TotalValue { get; set; }
        public Dictionary<string, Price> CurrentInvestmentPrices { get; } = new();
        public Dictionary<string, double> InvestmentCounts { get; } = new();


    }

    private IProjectionStore<string, PortfolioStatus> _projectionStore;

    public PortfolioStatusProjection(IProjectionStore<string, PortfolioStatus> projectionStore)
    {
        _projectionStore = projectionStore;
    }

    public Task Handle(NewTransactionWasCreated @event, CancellationToken token)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        var investmentId = @event.InvestmentId;
        var newPrice = new Price(@event.Price, Enumeration.FromDisplayName<CurrencyType>(@event.Currency));
        if (!projection.CurrentInvestmentPrices.ContainsKey(investmentId))
        {
            projection.CurrentInvestmentPrices.Add(investmentId, newPrice);
        }
        else
        {
            projection.CurrentInvestmentPrices[investmentId] = newPrice;
        }

        if (!projection.InvestmentCounts.ContainsKey(investmentId))
        {
            projection.InvestmentCounts.Add(investmentId, 0);
        }

        projection.InvestmentCounts[investmentId] +=
            (Enumeration.FromDisplayName<TransactionType>(@event.TransactionType) == TransactionType.Sale ? -1 : 1)
            * @event.Amount;

        projection.TotalValue = projection.InvestmentCounts
            .Sum(g => (decimal)g.Value * projection.CurrentInvestmentPrices[g.Key].Value)
            .ToString("C", new CultureInfo("no-NO"));

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }
}