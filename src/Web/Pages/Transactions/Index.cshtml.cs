using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs;
using PocketCqrs.EventStore;

namespace Web.Pages.Transactions;

public class Index : PageModel
{
    private readonly IMediator _messaging;
    public Model Data { get; set; }

    public Index(IMediator messaging) => _messaging = messaging;

    public async Task OnGetAsync(Query query)
    {
        Data = await _messaging.Send(query);
    }

    public class Query : IRequest<Model>
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

    public class QueryHandler : IRequestHandler<Query, Model>
    {
        public QueryHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public async Task<Model> Handle(Query query, CancellationToken token)
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
                        Price = t.Price.Value,
                        Fee = t.Fee.Value,
                        Currency = t.Price.Currency.DisplayName
                    }).ToList()
            };
        }
    }
}