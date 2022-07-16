using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs.Projections;
using Domain.Projections;

namespace Web.Pages.Distribution;

public class Index : PageModel
{
    private readonly IMediator _messaging;
    public PortfolioProjection Data { get; set; }

    public Index(IMediator messaging) => _messaging = messaging;

    public async Task OnGet(Query query)
    {
        Data = await _messaging.Send(query);
    }

    public class Query : IRequest<PortfolioProjection>
    {
        public string PortfolioId { get; set; }
    }

    public class QueryHandler : IRequestHandler<Query, PortfolioProjection>
    {
        private IProjectionStore<string, PortfolioProjection> _projectionStore;

        public QueryHandler(IProjectionStore<string, PortfolioProjection> projectionStore)
        {
            _projectionStore = projectionStore;
        }

        public Task<PortfolioProjection> Handle(Query query, CancellationToken token)
        {
            var portfolioProjection = _projectionStore.GetProjection(query.PortfolioId);
            return Task.FromResult(portfolioProjection);
        }
    }
}