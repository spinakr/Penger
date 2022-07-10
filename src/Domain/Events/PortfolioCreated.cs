using MediatR;
using PocketCqrs;

namespace Domain.Events
{
    public class PortfolioCreated : IEvent, INotification
    {
        public PortfolioCreated(string portfolioId)
        {
            PortfolioId = portfolioId;
        }
        public string PortfolioId { get; }
    }
}