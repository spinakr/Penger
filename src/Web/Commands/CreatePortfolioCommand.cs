using Domain;
using MediatR;
using PocketCqrs.EventStore;

namespace Web.Commands;

public class CreatePortfolioCommand : IRequest<string>
{
    public string Name { get; set; }
}

public class CreatePortfolioCommandHandler : IRequestHandler<CreatePortfolioCommand, string>
{
    public CreatePortfolioCommandHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    private IEventStore _eventStore { get; }

    public Task<string> Handle(CreatePortfolioCommand cmd, CancellationToken token)
    {
        var newPortfolio = Portfolio.CreateNew(cmd.Name);
        _eventStore.AppendToStream(newPortfolio.Id.ToString(), newPortfolio.PendingEvents, 0);
        return Task.FromResult(newPortfolio.Id);
    }
}