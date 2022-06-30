using PocketCqrs;

namespace Domain.Events
{
    public class NewTransactionWasCreated : IEvent
    {
        public DateTime Date { get; set; }
        public string InvestmentId { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Fee { get; set; }
        public string Currency { get; set; }

    }
}