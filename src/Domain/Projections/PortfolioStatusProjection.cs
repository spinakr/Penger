using System.Globalization;
using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using PocketCqrs.Projections;

namespace Domain.Projections;

public class PortfolioStatus
{
    public string PortfolioId { get; set; }
    public string TotalValue { get; set; }
    public Dictionary<string, InvestmentStatus> InvestmentStatuses { get; set; } = new();
    public Dictionary<string, Percent> DesiredDistribution { get; set; } = new();
    public Dictionary<string, Percent> ActualDistribution { get; set; } = new();
    public Dictionary<string, RegisteredInvestment> RegisteredInvestments { get; set; } = new();


    public record InvestmentStatus(string InvestmentId, double Amount, decimal Price, decimal Value);

    public record RegisteredInvestment(string InvestmentId, string InvestmentGroup);

}

public class PortfolioStatusProjection :
    INotificationHandler<NewTransactionWasCreated>,
    INotificationHandler<InvestmentPriceWasUpdated>,
    INotificationHandler<InvestmentWasRegistered>,
    INotificationHandler<PortfolioDistributionWasChanged>
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

        AddNewProjectionStatus(projection, investmentId, newAmount, newPrice);
        CalculateTotal(projection);
        CalculateDistribution(projection);

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

        AddNewProjectionStatus(projection, investmentId, currentAmount, newPrice);

        CalculateTotal(projection);
        CalculateDistribution(projection);

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }

    public Task Handle(PortfolioDistributionWasChanged notification, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(notification.PortfolioId);
        projection.DesiredDistribution = notification.Distribution.ToDictionary(
            x => x.Key, x => new Percent(x.Value));
        _projectionStore.Save(notification.PortfolioId, projection);
        return Task.CompletedTask;
    }

    public Task Handle(InvestmentWasRegistered notification, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(notification.PortfolioId);
        projection.RegisteredInvestments.Add(notification.InvestmentId,
            new PortfolioStatus.RegisteredInvestment(notification.InvestmentId, notification.InvestmentGroup));
        _projectionStore.Save(notification.PortfolioId, projection);
        return Task.CompletedTask;
    }
    private static void AddNewProjectionStatus(PortfolioStatus projection, string investmentId, double newAmount, decimal newPrice)
    {
        var newInvestmentStatus = new PortfolioStatus.InvestmentStatus(
            InvestmentId: investmentId,
            Price: newPrice,
            Amount: newAmount,
            Value: newPrice * (decimal)newAmount);

        projection.InvestmentStatuses[investmentId] = newInvestmentStatus;

        if (newInvestmentStatus.Value < 100) projection.InvestmentStatuses.Remove(investmentId);
    }

    private static void CalculateDistribution(PortfolioStatus projection)
    {
        var totalValue = projection.InvestmentStatuses.Values.Sum(g => g?.Value ?? 0);
        projection.ActualDistribution = projection.InvestmentStatuses
            .GroupBy(x => projection.RegisteredInvestments[x.Key].InvestmentGroup)
            .ToDictionary(x => x.Key, x => new Percent(x.Sum(y => y.Value.Value) / totalValue));

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