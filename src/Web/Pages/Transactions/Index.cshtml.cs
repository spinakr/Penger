using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs;
using PocketCqrs.EventStore;

namespace Web.Pages.Transactions;

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

    public record Model
    {
        public List<Transaction> Transactions { get; set; }

        public record Transaction
        {
            public DateTime Date { get; set; }
            public string TransactionId { get; set; }
            public string InvestmentId { get; set; }
            public double Amount { get; set; }
            public decimal Price { get; set; }
            public decimal Fee { get; set; }
            public string Currency { get; set; }
        }
    }

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

            return new Model
            {
                Transactions = portfolio.Transactions.Select(t =>
                    new Model.Transaction
                    {
                        InvestmentId = t.InvestmentId.Value,
                        TransactionId = t.TransactionId.Value.ToString(),
                        Date = t.Date,
                        Amount = t.Amount,
                        Price = t.Price,
                        Fee = t.Fee,
                        Currency = t.Currency
                    }).ToList()
            };
        }
    }
}