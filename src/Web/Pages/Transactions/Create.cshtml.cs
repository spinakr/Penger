using Domain;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PocketCqrs.EventStore;

namespace Web.Pages.Transactions;

public class Create : PageModel
{
    private readonly IMediator _messaging;
    [BindProperty]
    public Model Data { get; set; }

    public Create(IMediator messaging) => _messaging = messaging;


    public async Task OnGetAsync(Query q) => Data = await _messaging.Send(q);

    public async Task<IActionResult> OnPostAsync()
    {
        var model = new List<Index.Model.Transaction>
        {
            await _messaging.Send(Data.command)
        };
        return ViewComponent("Transactions", model);
    }

    public class Model
    {
        public Command command { get; set; }
        public List<SelectListItem> InvestmentOptions { get; set; }
    }

    public record Query(string portfolioId) : IRequest<Model>;

    public class Command : IRequest<Index.Model.Transaction>
    {
        public string PortfolioId { get; set; }
        public string InvestmentId { get; set; }
        public DateTime Date { get; set; }
        public double? Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal Fee { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }


    }

    public class InvestmentQueryHandler : IRequestHandler<Query, Model>
    {
        public InvestmentQueryHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Task<Model> Handle(Query query, CancellationToken token)
        {
            var stream = _eventStore.LoadEventStream(query.portfolioId);
            var portfolio = new Portfolio(stream.Events);
            return Task.FromResult(new Model
            {
                command = new Command
                {
                    PortfolioId = query.portfolioId,
                    Date = DateTime.Now,
                    Fee = 0,
                },
                InvestmentOptions = portfolio.RegisteredInvestments.Select(i => new SelectListItem(i.Id.ToString(), i.Id.ToString())).ToList()
            });
        }
    }


    public class CommandHandler : IRequestHandler<Command, Index.Model.Transaction>
    {
        public CommandHandler(IEventStore eventStore, IMediator messaging)
        {
            _eventStore = eventStore;
            _messaging = messaging;
        }

        private IEventStore _eventStore { get; }
        private IMediator _messaging { get; }

        public Task<Index.Model.Transaction> Handle(Command cmd, CancellationToken token)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            var newTransaction = new Transaction(
                new InvestmentId(cmd.InvestmentId),
                new TransactionId(),
                cmd.Date,
                new Amount(cmd.Amount ?? 0),
                new Money(cmd.Price ?? 0, Enumeration.FromDisplayName<CurrencyType>(cmd.Currency)),
                new Money(cmd.Fee, Enumeration.FromDisplayName<CurrencyType>(cmd.Currency)),
                string.IsNullOrWhiteSpace(cmd.Type) ? null : Enumeration.FromDisplayName<TransactionType>(cmd.Type)
            );
            portfolio.AddTransaction(newTransaction);

            _eventStore.AppendToStream(portfolio.Id, portfolio.PendingEvents, stream.Version);

            return Task.FromResult(new Index.Model.Transaction
            {
                TransactionId = newTransaction.TransactionId.Value.ToString(),
                InvestmentId = newTransaction.InvestmentId.Value.ToString(),
                Date = newTransaction.Date,
                Amount = newTransaction.Amount.Value,
                Price = newTransaction.Price.Value,
                Fee = newTransaction.Fee.Value,
                Currency = newTransaction.Price.Currency.DisplayName
            });
        }
    }
}