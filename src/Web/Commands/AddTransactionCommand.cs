using Domain;
using Domain.ValueObjects;
using PocketCqrs;
using PocketCqrs.EventStore;

namespace Web.Commands;

public class AddTransactionCommand : ICommand
{
    public string PortfolioId { get; set; }
    public DateTime Date { get; set; }
    public string InvestmentId { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public decimal Fee { get; set; }
    public string Currency { get; set; }
}

public class AddTransactionCommandHandler : ICommandHandler<AddTransactionCommand>
{
    public AddTransactionCommandHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    private IEventStore _eventStore { get; }

    public Result Handle(AddTransactionCommand cmd)
    {
        var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
        var portfolio = new Portfolio(stream.Events);

        portfolio.AddTransaction(
            Transaction.CreateNew(
                new InvestmentId(cmd.InvestmentId),
                cmd.Date,
                cmd.Amount,
                cmd.Price,
                cmd.Fee,
                cmd.Currency
            ));

        _eventStore.AppendToStream(portfolio.Id.ToString(), portfolio.PendingEvents, stream.Version);
        return Result.Complete();
    }
}