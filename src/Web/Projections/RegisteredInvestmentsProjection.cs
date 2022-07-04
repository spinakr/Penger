using Domain.Events;
using MediatR;
using PocketCqrs.Projections;

namespace Web.Projections;

public record RegisteredInvestment(string InvestmentId, string InvestmentType, string InvestmentGroup);

public class RegisteredInvestmentsProjection : INotificationHandler<InvestmentWasRegistered>
{
    private IProjectionStore<string, List<RegisteredInvestment>> _projectionStore;

    public RegisteredInvestmentsProjection(IProjectionStore<string, List<RegisteredInvestment>> projectionStore)
    {
        _projectionStore = projectionStore;
    }

    public Task Handle(InvestmentWasRegistered @event, CancellationToken token)
    {
        var projection = _projectionStore.GetProjection(@event.PortfolioId);

        projection.Add(new RegisteredInvestment(@event.InvestmentId, @event.InvestmentType, @event.InvestmentGroup));

        _projectionStore.Save(@event.PortfolioId, projection);

        return Task.CompletedTask;
    }
}