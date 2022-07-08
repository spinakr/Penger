using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs.Projections;
using Domain.Projections;

namespace Web.Pages.Investments;

public class Index : PageModel
{
    private readonly IMediator _messaging;
    public Model Data { get; set; }

    public Index(IMediator messaging) => _messaging = messaging;

    public record Model
    {
        public List<RegisteredInvestment> Investments { get; set; }
    }

    public class Query : IRequest<Model>
    {
        public string PortfolioId { get; set; }
    }

    public async Task OnGetAsync(Query query)
    {
        Data = await _messaging.Send(query);
    }

    public class QueryHandler : IRequestHandler<Query, Model>
    {
        private IProjectionStore<string, List<RegisteredInvestment>> _projectionStore;
        public QueryHandler(IProjectionStore<string, List<RegisteredInvestment>> projectionStore)
        {
            _projectionStore = projectionStore;
        }

        public Task<Model> Handle(Query query, CancellationToken token)
        {
            var projection = _projectionStore.GetProjection(query.PortfolioId);

            return Task.FromResult(new Model
            {
                Investments = projection
            });
        }
    }
}