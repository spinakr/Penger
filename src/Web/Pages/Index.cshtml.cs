using System.Globalization;
using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs;
using PocketCqrs.EventStore;

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
        public QueryHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Model Handle(Query query)
        {
            var stream = _eventStore.LoadEventStream(query.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            return new Model(query.PortfolioId, Math.Floor(portfolio.TotalPortfolioValue).ToString("C", new CultureInfo("no-NO")));
        }
    }
}
