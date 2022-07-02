using System.ComponentModel.DataAnnotations;
using Domain;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PocketCqrs;
using PocketCqrs.EventStore;

namespace Web.Pages.Transactions;

public class Create : PageModel
{
    private readonly IMessaging _messaging;
    [BindProperty]
    public Model Data { get; set; }

    public Create(IMessaging messaging) => _messaging = messaging;


    public void OnGet(Query q) => Data = _messaging.Dispatch(q);

    public IActionResult OnPost()
    {
        var model = new List<Index.Model.Transaction>
        {
            ((Result<Index.Model.Transaction>)_messaging.Dispatch(Data.command)).Value
        };
        return ViewComponent("Transactions", model);
        // return RedirectToPage("/Transactions/Index");
    }

    public class Model
    {
        public Command command { get; set; }
        public List<SelectListItem> InvestmentOptions { get; set; }
    }

    public record Query(string portfolioId) : IQuery<Model>;

    public class Command : ICommand
    {
        public string PortfolioId { get; set; }
        public string InvestmentId { get; set; }
        public DateTime Date { get; set; }
        public double? Amount { get; set; }
        public decimal? Price { get; set; }
        public decimal Fee { get; set; }
        public string Currency { get; set; }
    }

    public class InvestmentQueryHandler : IQueryHandler<Query, Model>
    {
        public InvestmentQueryHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Model Handle(Query query)
        {
            var stream = _eventStore.LoadEventStream(query.portfolioId);
            var portfolio = new Portfolio(stream.Events);
            return new Model
            {
                command = new Command
                {
                    PortfolioId = query.portfolioId,
                    Date = DateTime.Now,
                    Fee = 0,
                },
                InvestmentOptions = portfolio.RegisteredInvestments.Select(i => new SelectListItem(i.Id.ToString(), i.Id.ToString())).ToList()
            };
        }
    }


    public class CommandHandler : ICommandHandler<Command>
    {
        public CommandHandler(IEventStore eventStore, IMessaging messaging)
        {
            _eventStore = eventStore;
            _messaging = messaging;
        }

        private IEventStore _eventStore { get; }
        private IMessaging _messaging { get; }

        public Result Handle(Command cmd)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            var newTransaction = Transaction.CreateNew(
                new InvestmentId(cmd.InvestmentId),
                cmd.Date,
                cmd.Amount ?? 0,
                new Price(cmd.Price ?? 0, Enumeration.FromDisplayName<CurrencyType>(cmd.Currency)),
                new Price(cmd.Fee, Enumeration.FromDisplayName<CurrencyType>(cmd.Currency))
            );
            portfolio.AddTransaction(newTransaction);

            _eventStore.AppendToStream(portfolio.Id, portfolio.PendingEvents, stream.Version);

            foreach (var e in portfolio.PendingEvents)
            {
                _messaging.Publish(e);
            }

            return Result.Complete(new Index.Model.Transaction
            {
                TransactionId = newTransaction.TransactionId.Value.ToString(),
                InvestmentId = newTransaction.InvestmentId.Value.ToString(),
                Date = newTransaction.Date,
                Amount = newTransaction.Amount,
                Price = newTransaction.Price.Value,
                Fee = newTransaction.Fee.Value,
                Currency = newTransaction.Price.Currency.DisplayName
            });
        }
    }
}