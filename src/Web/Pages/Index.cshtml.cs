using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;
using Web.Projections;

namespace Web.Pages;

public class Index : PageModel
{
    private readonly IMessaging _messaging;
    public Model Data { get; set; }

    public Index(IMessaging messaging) => _messaging = messaging;

    public void OnGet(Query query)
    {
        Data = _messaging.Dispatch(query);
    }

    public class Query : IQuery<Model>
    {
        public string PortfolioId { get; set; }
    }

    public record Model(string portfolioId, string totalValue);

    public class QueryHandler : IQueryHandler<Query, Model>
    {
        private IEventStore _eventStore { get; }
        private IProjectionStore<string, PortfolioStatusProjection.PortfolioStatus> _projectionStore { get; }
        public QueryHandler(IEventStore eventStore, IProjectionStore<string, PortfolioStatusProjection.PortfolioStatus> projectionStore)
        {
            _eventStore = eventStore;
            _projectionStore = projectionStore;
        }

        public Model Handle(Query query)
        {
            var portfolioProjection = _projectionStore.GetProjection(query.PortfolioId);
            return new Model(query.PortfolioId, portfolioProjection.TotalValue);
        }
    }
}
