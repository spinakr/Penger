using System.Globalization;
using Domain;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using PocketCqrs.Projections;

namespace Web.Projections;

public class PortfolioStatus
{
    public string PortfolioId { get; set; }
    public string TotalValue { get; set; }
    public Dictionary<string, decimal> InvestmentValues { get; } = new();
    public Dictionary<string, Price> CurrentInvestmentPrices { get; } = new();
    public Dictionary<string, double> InvestmentCounts { get; } = new();
}

public class PortfolioStatusProjection : INotificationHandler<NewTransactionWasCreated>, INotificationHandler<InvestmentPriceWasUpdated>
{

    private IProjectionStore<string, PortfolioStatus> _projectionStore;

    public PortfolioStatusProjection(IProjectionStore<string, PortfolioStatus> projectionStore)
    {
        _projectionStore = projectionStore;
    }

    public Task Handle(NewTransactionWasCreated @event, CancellationToken token)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        var investmentId = @event.InvestmentId;
        projection.PortfolioId = @event.PortfolioId;
        var newPrice = new Price(@event.Price, Enumeration.FromDisplayName<CurrencyType>(@event.Currency));
        UpdateInvestmentPrice(projection, investmentId, newPrice);

        UpdateInvestmentCounts(@event, projection, investmentId);

        UpdateInvestmentValues(projection, investmentId);

        CalculateTotal(projection);

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }


    public Task Handle(InvestmentPriceWasUpdated @event, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);
        var newPrice = new Price(@event.NokPrice, CurrencyType.NOK);
        UpdateInvestmentPrice(projection, @event.InvestmentId, newPrice);
        CalculateTotal(projection);
        UpdateInvestmentValues(projection, @event.InvestmentId);
        _projectionStore.Save(@event.PortfolioId, projection);
        return Task.CompletedTask;
    }

    private static void UpdateInvestmentValues(PortfolioStatus projection, string investmentId)
    {
        if (!projection.InvestmentValues.ContainsKey(investmentId))
        {
            projection.InvestmentValues.Add(investmentId, 0);
        }

        projection.InvestmentValues[investmentId] = (decimal)projection.InvestmentCounts[investmentId] * projection.CurrentInvestmentPrices[investmentId].Value;
    }

    private static void UpdateInvestmentCounts(NewTransactionWasCreated @event, PortfolioStatus projection, string investmentId)
    {
        if (!projection.InvestmentCounts.ContainsKey(investmentId))
        {
            projection.InvestmentCounts.Add(investmentId, 0);
        }

        projection.InvestmentCounts[investmentId] +=
            (Enumeration.FromDisplayName<TransactionType>(@event.TransactionType) == TransactionType.Sale ? -1 : 1)
            * @event.Amount;
    }

    private static void CalculateTotal(PortfolioStatus projection)
    {
        projection.TotalValue = projection.InvestmentCounts
            .Sum(g => (decimal)g.Value * projection.CurrentInvestmentPrices[g.Key].Value)
            .ToString("C", new CultureInfo("no-NO"));
    }

    private static void UpdateInvestmentPrice(PortfolioStatus projection, string investmentId, Price newPrice)
    {
        if (!projection.CurrentInvestmentPrices.ContainsKey(investmentId))
        {
            projection.CurrentInvestmentPrices.Add(investmentId, newPrice);
        }
        else
        {
            projection.CurrentInvestmentPrices[investmentId] = newPrice;
        }
    }
}