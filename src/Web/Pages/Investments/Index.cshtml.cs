using System.ComponentModel.DataAnnotations;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs.EventStore;

namespace Web.Pages.Investments;

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
        public List<Investment> Investments { get; set; }

        public record Investment
        {
            [Display(Name = "Investment Id")]
            public string InvestmendId { get; set; }
            [Display(Name = "Investment Type")]
            public string InvestmentType { get; set; }
            [Display(Name = "Investment Group")]
            public string InvestmentGroup { get; set; }
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
            System.Console.WriteLine("Loading events for " + query.PortfolioId);
            var stream = _eventStore.LoadEventStream(query.PortfolioId);
            var portfolio = new Portfolio(stream.Events);

            return new Model
            {
                Investments = portfolio.RegisteredInvestments.Select(i =>
                    new Model.Investment
                    {
                        InvestmendId = i.Id.Value,
                        InvestmentGroup = i.Group.DisplayName,
                        InvestmentType = i.Type.DisplayName
                    }).ToList()
            };
        }
    }
}