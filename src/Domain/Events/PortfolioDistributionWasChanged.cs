using MediatR;
using PocketCqrs;

namespace Domain.Events
{
    public class PortfolioDistributionWasChanged : INotification, IEvent
    {
        public PortfolioDistributionWasChanged(string portfolioId, Dictionary<string, double> distribution)
        {
            PortfolioId = portfolioId;
            Distribution = distribution;
        }

        public string PortfolioId { get; set; }
        public Dictionary<string, double> Distribution { get; set; }
    }
}