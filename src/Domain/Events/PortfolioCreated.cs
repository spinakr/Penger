using MediatR;
using PocketCqrs;

namespace Domain.Events
{
    public class PortfolioCreated : IEvent, INotification
    {
        public string PortfolioId { get; set; }
    }
}