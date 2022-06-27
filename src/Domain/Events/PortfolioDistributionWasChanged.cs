using PocketCqrs;

namespace Domain.Events
{
    public class PortfolioDistributionWasChanged : IEvent
    {
        public Dictionary<string, double> Distribution { get; set; }
    }
}