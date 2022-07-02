using PocketCqrs;

namespace Domain.Events
{
    public class PortfolioDistributionWasChanged : IEvent
    {
        public string PortfolioId { get; set; }
        public Dictionary<string, double> Distribution { get; set; }
    }
}