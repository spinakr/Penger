using PocketCqrs;
using PocketCqrs.EventStore;

public class CreatePortfolioCommand : ICommand
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; }

    public CreatePortfolioCommand(Guid customerId, string name)
    {
        CustomerId = customerId;
        Name = name;
    }
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
        // var newAccount = Account.CreateNew(cmd.Name, cmd.CustomerId);
        // _eventStore.AppendToStream(newAccount.Id.ToString(), newAccount.PendingEvents, 0);
        return Result.Complete();
    }
}