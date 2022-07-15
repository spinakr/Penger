using Domain.Events;
using Domain.ValueObjects;
using MediatR;
using PocketCqrs.Projections;

namespace Domain.Projections;

public record RegisteredInvestmentsProjection(InvestmentId InvestmentId, InvestmentType InvestmentType, InvestmentGroup InvestmentGroup, Symbol Symbol, CurrencyType Currency);

public class RegisteredInvestmentsProjector : INotificationHandler<InvestmentWasRegistered>
{
    private IProjectionStore<string, List<RegisteredInvestmentsProjection>> _projectionStore;

    public RegisteredInvestmentsProjector(IProjectionStore<string, List<RegisteredInvestmentsProjection>> projectionStore)
    {
        _projectionStore = projectionStore;
    }

    public Task Handle(InvestmentWasRegistered @event, CancellationToken token)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        projection.Add(new RegisteredInvestmentsProjection(
            new InvestmentId(@event.InvestmentId),
            Enumeration.FromDisplayName<InvestmentType>(@event.InvestmentType),
            Enumeration.FromDisplayName<InvestmentGroup>(@event.InvestmentGroup),
            new Symbol(@event.Symbol),
            Enumeration.FromDisplayName<CurrencyType>(@event.Currency)));

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }
}