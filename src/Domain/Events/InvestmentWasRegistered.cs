using PocketCqrs;

namespace Domain.Events
{
    public class InvestmentWasRegistered : IEvent
    {
        public string InvestmentId { get; set; }
        public string InvestmentType { get; set; }
        public string InvestmentGroup { get; set; }
    }
}