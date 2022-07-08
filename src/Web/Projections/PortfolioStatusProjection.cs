using System.Globalization;
using Domain;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using Newtonsoft.Json;
using PocketCqrs.Projections;

namespace Web.Projections;

public class PortfolioStatus
{
    public string PortfolioId { get; set; }
    public string TotalValue { get; set; }
    public Dictionary<string, InvestmentStatus> InvestmentStatuses { get; set; } = new();

}
public record InvestmentStatus(string InvestmentId, double Amount, decimal Price, decimal Value);

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

        projection.PortfolioId = @event.PortfolioId;
        var investmentId = @event.InvestmentId;

        var transactionPrice = new Price(@event.Price, Enumeration.FromDisplayName<CurrencyType>(@event.Currency));
        var currentStatus = projection.InvestmentStatuses.GetValueOrDefault(investmentId);
        var currentAmount = currentStatus?.Amount ?? 0;
        var newAmount = currentAmount + (@event.TransactionType == TransactionType.Sale.DisplayName ? -1 : 1) * @event.Amount;
        var newPrice = currentStatus is null ? transactionPrice.Value : currentStatus.Price;

        var newInvestmentStatus = new InvestmentStatus(
            InvestmentId: investmentId,
            Price: newPrice,
            Amount: newAmount,
            Value: newPrice * (decimal)newAmount);

        projection.InvestmentStatuses[investmentId] = newInvestmentStatus;

        CalculateTotal(projection);

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }


    public Task Handle(InvestmentPriceWasUpdated @event, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        var investmentId = @event.InvestmentId;
        projection.PortfolioId = @event.PortfolioId;

        var transactionPrice = new Price(@event.NokPrice, CurrencyType.NOK);
        var currentStatus = projection.InvestmentStatuses.GetValueOrDefault(investmentId);
        var currentAmount = currentStatus?.Amount ?? 0;
        var newPrice = transactionPrice.Value;

        var newInvestmentStatus = new InvestmentStatus(
            InvestmentId: investmentId,
            Price: newPrice,
            Amount: currentAmount,
            Value: newPrice * (decimal)currentAmount);

        projection.InvestmentStatuses[investmentId] = newInvestmentStatus;

        CalculateTotal(projection);

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }


    private static void CalculateTotal(PortfolioStatus projection)
    {
        projection.TotalValue = projection.InvestmentStatuses.Values
            .Sum(g => g?.Value ?? 0)
            .ToString("C", new CultureInfo("no-NO"));

        projection.InvestmentStatuses = projection.InvestmentStatuses
            .OrderByDescending(g => g.Value.Value)
            .ToDictionary(g => g.Key, g => g.Value);
    }

}