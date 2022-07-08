using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;
using Domain.Projections;

namespace Web.Pages;

public class Index : PageModel
{
    private readonly IMediator _messaging;
    public PortfolioStatus Data { get; set; }

    public Index(IMediator messaging) => _messaging = messaging;

    public async Task OnGet(Query query)
    {
        Data = await _messaging.Send(query);
    }

    public class Query : IRequest<PortfolioStatus>
    {
        public string PortfolioId { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, PortfolioStatus>
    {
        private IEventStore _eventStore { get; }
        private IProjectionStore<string, PortfolioStatus> _projectionStore { get; }
        public QueryHandler(
            IEventStore eventStore,
            IProjectionStore<string, PortfolioStatus> projectionStore)
        {
            _eventStore = eventStore;
            _projectionStore = projectionStore;
        }

        public Task<PortfolioStatus> Handle(Query query, CancellationToken token)
        {
            var portfolioProjection = _projectionStore.GetProjection(query.PortfolioId);
            return Task.FromResult(portfolioProjection);
        }
    }
}
