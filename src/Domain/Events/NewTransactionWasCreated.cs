using PocketCqrs;

namespace Domain.Events
{
    public class NewTransactionWasCreated : IEvent
    {
        public string PortfolioId { get; set; }
        public DateTime Date { get; set; }
        public string InvestmentId { get; set; }
        public Guid TransactionId { get; set; }
        public string TransactionType { get; set; }
        public double Amount { get; set; }
        public decimal Price { get; set; }
        public decimal Fee { get; set; }
        public string Currency { get; set; }

    }
}