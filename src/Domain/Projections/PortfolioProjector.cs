using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using PocketCqrs.Projections;

namespace Domain.Projections;

public class PortfolioProjection
{
    public string PortfolioId { get; set; }
    public Money TotalValue { get; set; }
    public Money TotalProfitValue { get; set; }
    public Percent TotalProfit { get; set; }


    public Dictionary<InvestmentId, InvestmentStatus> InvestmentStatuses { get; set; } = new();
    public Dictionary<InvestmentGroup, Percent> DesiredDistribution { get; set; } = new();
    public Dictionary<InvestmentGroup, Percent> ActualDistribution { get; set; } = new();
    public Dictionary<InvestmentId, InvestmentGroup> RegisteredInvestments { get; set; } = new();


    public record InvestmentStatus(InvestmentId InvestmentId, Amount Amount, Money Price, Money Value, Money Invested, Money ProfitValue, Percent Profit);
}

public class PortfolioProjector :
    INotificationHandler<NewTransactionWasCreated>,
    INotificationHandler<InvestmentPriceWasUpdated>,
    INotificationHandler<InvestmentWasRegistered>,
    INotificationHandler<PortfolioDistributionWasChanged>
{

    private IProjectionStore<string, PortfolioProjection> _projectionStore;

    public PortfolioProjector(IProjectionStore<string, PortfolioProjection> projectionStore)
    {
        _projectionStore = projectionStore;
    }

    public Task Handle(NewTransactionWasCreated @event, CancellationToken token)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        projection.PortfolioId = @event.PortfolioId;
        var investmentId = new InvestmentId(@event.InvestmentId);

        var transactionCurrency = Enumeration.FromDisplayName<CurrencyType>(@event.Currency);
        var transactionPrice = new Money(@event.Price, transactionCurrency);
        var currentStatus = projection.InvestmentStatuses.GetValueOrDefault(investmentId);
        var currentAmount = currentStatus?.Amount ?? new Amount(0);
        var currentInvested = currentStatus?.Invested ?? new Money(0, transactionCurrency);
        var newInvested =@event.TransactionType == TransactionType.Sale.DisplayName ?
            currentInvested - new Money(transactionPrice.Value, transactionCurrency) * new Amount(@event.Amount) :
            currentInvested + new Money(transactionPrice.Value, transactionCurrency) * new Amount(@event.Amount);
        var newAmount = @event.TransactionType == TransactionType.Sale.DisplayName ? 
            currentAmount - new Amount(@event.Amount) : currentAmount + new Amount(@event.Amount);
        var newPrice = currentStatus is null ? transactionPrice : currentStatus.Price;

        AddNewProjectionStatus(projection, investmentId, newAmount, newPrice, newInvested);
        CalculateTotal(projection);
        CalculateDistribution(projection);

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }


    public Task Handle(InvestmentPriceWasUpdated @event, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        projection.PortfolioId = @event.PortfolioId;
        var investmentId = new InvestmentId(@event.InvestmentId);

        var transactionPrice = new Money(@event.NokPrice, CurrencyType.NOK);
        var currentStatus = projection.InvestmentStatuses.GetValueOrDefault(investmentId);
        var currentAmount = currentStatus?.Amount ?? new Amount(0);
        var currentInvested = currentStatus?.Invested ?? new Money(0, CurrencyType.NOK);

        AddNewProjectionStatus(projection, investmentId, currentAmount, transactionPrice, currentInvested);

        CalculateTotal(projection);
        CalculateDistribution(projection);

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }

    public Task Handle(PortfolioDistributionWasChanged notification, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(notification.PortfolioId);
        projection.DesiredDistribution = notification.Distribution.ToDictionary(
            x => Enumeration.FromDisplayName<InvestmentGroup>(x.Key),
            x => new Percent(x.Value));
        _projectionStore.Save(notification.PortfolioId, projection);
        return Task.CompletedTask;
    }

    public Task Handle(InvestmentWasRegistered notification, CancellationToken cancellationToken)
    {
        var projection = _projectionStore.GetProjection(notification.PortfolioId);
        projection.RegisteredInvestments.Add(
            new InvestmentId(notification.InvestmentId),
            Enumeration.FromDisplayName<InvestmentGroup>(notification.InvestmentGroup));
        _projectionStore.Save(notification.PortfolioId, projection);
        return Task.CompletedTask;
    }
    private static void AddNewProjectionStatus(PortfolioProjection projection, InvestmentId investmentId, Amount newAmount, Money newPrice, Money invested)
    {
        var profitValue = newPrice * newAmount - invested;
        var profit = invested.IsZero ? Percent.Zero : profitValue.PercentOf(invested);
        var newInvestmentStatus = new PortfolioProjection.InvestmentStatus(
            InvestmentId: investmentId,
            Price: newPrice,
            Amount: newAmount,
            Value: newPrice * newAmount,
            Invested: invested,
            ProfitValue: profitValue,
            Profit: profit);

        projection.InvestmentStatuses[investmentId] = newInvestmentStatus;

        if (newInvestmentStatus.Value.Value < 100) projection.InvestmentStatuses.Remove(investmentId);

        projection.InvestmentStatuses = projection.InvestmentStatuses
            .OrderByDescending(g => g.Value.Value.Value)
            .ToDictionary(g => g.Key, g => g.Value);
    }

    private static void CalculateDistribution(PortfolioProjection projection)
    {
        var totalValue = projection.InvestmentStatuses.Values.Sum(g => g.Value.Value);
        projection.ActualDistribution = projection.InvestmentStatuses
            .GroupBy(x => projection.RegisteredInvestments[x.Key])
            .ToDictionary(x => x.Key, x => new Percent(x.Sum(y => y.Value.Value.Value / totalValue)));
    }

    private static void CalculateTotal(PortfolioProjection projection)
    {
        projection.TotalValue = new Money(projection.InvestmentStatuses.Values.Sum(g => g?.Value.Value ?? 0), CurrencyType.NOK);
        projection.TotalProfitValue = new Money(projection.InvestmentStatuses.Values.Sum(g => g?.ProfitValue.Value ?? 0), CurrencyType.NOK);
        var totalInvested = new Money(projection.InvestmentStatuses.Values.Sum(g => g?.Invested.Value ?? 0), CurrencyType.NOK);
        projection.TotalProfit = totalInvested.IsZero ? Percent.Zero : projection.TotalProfitValue.PercentOf(totalInvested);
    }
}