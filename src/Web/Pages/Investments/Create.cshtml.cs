using Domain;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PocketCqrs.EventStore;

namespace Web.Pages.Investments;

public class Create : PageModel
{
    private readonly IMediator _messaging;
    [BindProperty]
    public Command Data { get; set; }

    public List<SelectListItem> InvestmentTypeOptions = Enumeration.GetAll<InvestmentType>().Select(t => new SelectListItem(t.DisplayName, t.DisplayName)).ToList();
    public List<SelectListItem> InvestmentGroupOptions = Enumeration.GetAll<InvestmentGroup>().Select(t => new SelectListItem(t.DisplayName, t.DisplayName)).ToList();

    public Create(IMediator messaging) => _messaging = messaging;

    public record Query(string portfolioId);

    public void OnGet(Query q)
    {
        Data = new Command
        {
            PortfolioId = q.portfolioId
        };
    }

    public IActionResult OnPost()
    {
        _messaging.Send(Data);
        return RedirectToPage("/Investments/Index");
    }

    public class Command : IRequest
    {
        public string PortfolioId { get; set; }
        public string InvestmentId { get; set; }
        public string InvestmentGroup { get; set; }
        public string InvestmentType { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command>
    {
        public CommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Task<Unit> Handle(Command cmd, CancellationToken token)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            portfolio.RegisterInvestment(new Investment(cmd.InvestmentId, cmd.InvestmentType, cmd.InvestmentGroup));

            _eventStore.AppendToStream(portfolio.Id.ToString(), portfolio.PendingEvents, stream.Version);

            return Unit.Task;
        }
    }
}