using MediatR;
using PocketCqrs;

namespace Domain.Events
{
    public class InvestmentWasRegistered : IEvent, INotification
    {
        public string PortfolioId { get; set; }


        public string InvestmentId { get; set; }
        public string InvestmentType { get; set; }
        public string InvestmentGroup { get; set; }
    }
}