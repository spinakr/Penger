using Domain;
using PocketCqrs;
using PocketCqrs.EventStore;

namespace Web.Queries;

public class GetPortfolioAggregateQuery : IQuery<Portfolio>
{
    public string? PortfolioId { get; set; }
}

public class GetPortfolioAggregateQueryHandler : IQueryHandler<GetPortfolioAggregateQuery, Portfolio>
{
    public GetPortfolioAggregateQueryHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    private IEventStore _eventStore { get; }

    public Portfolio Handle(GetPortfolioAggregateQuery cmd)
    {
        var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
        var portfolio = new Portfolio(stream.Events);
        return portfolio;
    }
}