using Domain;
using Domain.Projections;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PocketCqrs.EventStore;

namespace Web.Pages.Distribution;

public class Update : PageModel
{
    private readonly IMediator _messaging;
    [BindProperty]
    public Command Data { get; set; }

    public Update(IMediator messaging) => _messaging = messaging;

    public record Query(string portfolioId);

    public async Task OnGetAsync(Query q)
    {
        Data = new Command
        {
            PortfolioId = q.portfolioId,
            Distribution = Enumeration.GetAll<InvestmentGroup>().ToDictionary(t => t.DisplayName, t => 0.0)
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _messaging.Send(Data);
        return ViewComponent("Distribution", Data.Distribution.ToDictionary(t => Enumeration.FromDisplayName<InvestmentGroup>(t.Key), t => new Percent(t.Value / 100)));
    }

    public class Command : IRequest<Dictionary<InvestmentGroup, Percent>>
    {
        public string PortfolioId { get; set; }
        public Dictionary<string, double> Distribution { get; set; }
    }

    public class CommandHandler : IRequestHandler<Command, Dictionary<InvestmentGroup, Percent>>
    {
        public CommandHandler(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        private IEventStore _eventStore { get; }

        public Task<Dictionary<InvestmentGroup, Percent>> Handle(Command cmd, CancellationToken token)
        {
            var stream = _eventStore.LoadEventStream(cmd.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            var newDistribution = cmd.Distribution.ToDictionary(t => Enumeration.FromDisplayName<InvestmentGroup>(t.Key), t => new Percent(t.Value / 100));
            portfolio.ChangeDistribution(newDistribution);

            _eventStore.AppendToStream(portfolio.Id.ToString(), portfolio.PendingEvents, stream.Version);

            return Task.FromResult(newDistribution);
        }
    }
}