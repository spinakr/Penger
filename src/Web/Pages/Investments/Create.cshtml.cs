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

    public async Task OnGetAsync(Query q)
    {
        Data = new Command
        {
            PortfolioId = q.portfolioId
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var model = new List<Index.Model.Investment>
        {
            await _messaging.Send(Data)
        };
        return ViewComponent("Investments", model);
    }

    public class Command : IRequest<Index.Model.Investment>
    {
        public string PortfolioId { get; set; }
        public string InvestmentId { get; set; }
        public string InvestmentGroup { get; set; }
        public string InvestmentType { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, Index.Model.Investment>
    {
        public CommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Task<Index.Model.Investment> Handle(Command cmd, CancellationToken token)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            var newInvestment = new Investment(cmd.InvestmentId, cmd.InvestmentType, cmd.InvestmentGroup);
            portfolio.RegisterInvestment(newInvestment);

            _eventStore.AppendToStream(portfolio.Id.ToString(), portfolio.PendingEvents, stream.Version);

            return Task.FromResult(new Index.Model.Investment
            {
                InvestmentId = cmd.InvestmentId,
                InvestmentType = cmd.InvestmentType,
                InvestmentGroup = cmd.InvestmentGroup
            });
        }
    }
}