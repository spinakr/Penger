using Domain;
using Domain.Projections;
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

    public async Task OnGetAsync(Query q)
    {
        Data = new Command
        {
            PortfolioId = q.portfolioId
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var model = new List<RegisteredInvestmentsProjection>
        {
            await _messaging.Send(Data)
        };
        return ViewComponent("Investments", model);
    }

    public class Command : IRequest<RegisteredInvestmentsProjection>
    {
        public string PortfolioId { get; set; }
        public string InvestmentId { get; set; }
        public string InvestmentGroup { get; set; }
        public string InvestmentType { get; set; }
        public string Symbol { get; set; }
        public string Currency { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, RegisteredInvestmentsProjection>
    {
        public CommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Task<RegisteredInvestmentsProjection> Handle(Command cmd, CancellationToken token)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            var newInvestment = new Investment(cmd.InvestmentId, cmd.InvestmentType, cmd.InvestmentGroup, cmd.Symbol, cmd.Currency);
            portfolio.RegisterInvestment(newInvestment);

            _eventStore.AppendToStream(portfolio.Id.ToString(), portfolio.PendingEvents, stream.Version);

            return Task.FromResult(new RegisteredInvestmentsProjection(
                new InvestmentId(cmd.InvestmentId),
                Enumeration.FromDisplayName<InvestmentType>(cmd.InvestmentType),
                Enumeration.FromDisplayName<InvestmentGroup>(cmd.InvestmentGroup),
                new Symbol(cmd.Symbol),
                Enumeration.FromDisplayName<CurrencyType>(cmd.Currency)));
        }
    }
}