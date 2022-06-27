using Domain;
using PocketCqrs;
using PocketCqrs.EventStore;

public class CreatePortfolioCommand : ICommand
{
}

public class CreatePortfolioCommandHandler : ICommandHandler<CreatePortfolioCommand>
{
    public CreatePortfolioCommandHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    private IEventStore _eventStore { get; }

    public Result Handle(CreatePortfolioCommand cmd)
    {
        var newPortfolio = Portfolio.CreateNew();
        _eventStore.AppendToStream(newPortfolio.Id.ToString(), newPortfolio.PendingEvents, 0);
        return Result.Complete<string>(newPortfolio.Id);
    }
}