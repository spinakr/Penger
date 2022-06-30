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
        _messaging.Dispatch(Data.command);
        return RedirectToPage("/Transactions/Index");
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
        public CommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Result Handle(Command cmd)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            portfolio.AddTransaction(Transaction.CreateNew(new InvestmentId(cmd.InvestmentId), cmd.Date, cmd.Amount ?? 0, cmd.Price ?? 0, cmd.Fee, cmd.Currency));

            _eventStore.AppendToStream(portfolio.Id.ToString(), portfolio.PendingEvents, stream.Version);
            return Result.Complete();
        }
    }
}