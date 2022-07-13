using MediatR;
using PocketCqrs;

namespace Domain.Events
{
    public class NewTransactionWasCreated : IEvent, INotification
    {
        public NewTransactionWasCreated(string portfolioId, DateTime date, string investmentId, Guid transactionId,
            double amount, decimal price, decimal fee, string currency, string transactionType)
        {
            PortfolioId = portfolioId;
            Date = date;
            InvestmentId = investmentId;
            TransactionId = transactionId;
            Amount = amount;
            Price = price;
            Currency = currency;
            TransactionType = transactionType;
            Fee = fee;
        }

        public string PortfolioId { get; }
        public DateTime Date { get; }
        public string InvestmentId { get; }
        public Guid TransactionId { get; }
        public string TransactionType { get; }
        public double Amount { get; }
        public decimal Price { get; }
        public decimal Fee { get; }
        public string Currency { get; }

    }
}