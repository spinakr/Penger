using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;
using Web.Projections;

namespace Web.Pages;

public class Index : PageModel
{
    private readonly IMediator _messaging;
    public Model Data { get; set; }

    public Index(IMediator messaging) => _messaging = messaging;

    public async Task OnGet(Query query)
    {
        Data = await _messaging.Send(query);
    }

    public class Query : IRequest<Model>
    {
        public string PortfolioId { get; set; }
    }

    public record Model(string portfolioId, string totalValue);

    public class QueryHandler : IRequestHandler<Query, Model>
    {
        private IEventStore _eventStore { get; }
        private IProjectionStore<string, PortfolioStatusProjection.PortfolioStatus> _projectionStore { get; }
        public QueryHandler(
            IEventStore eventStore,
            IProjectionStore<string, PortfolioStatusProjection.PortfolioStatus> projectionStore)
        {
            _eventStore = eventStore;
            _projectionStore = projectionStore;
        }

        public Task<Model> Handle(Query query, CancellationToken token)
        {
            var portfolioProjection = _projectionStore.GetProjection(query.PortfolioId);
            return Task.FromResult(new Model(query.PortfolioId, portfolioProjection.TotalValue));
        }
    }
}
