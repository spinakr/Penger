using PocketCqrs;

namespace Domain.Events
{
    public class PortfolioCreated : IEvent
    {
        public string PortfolioId { get; set; }

        public PortfolioCreated(string id)
        {
            PortfolioId = id;
        }
    }
}