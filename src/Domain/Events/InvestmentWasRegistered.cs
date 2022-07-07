using MediatR;
using PocketCqrs;

namespace Domain.Events
{
    public class InvestmentWasRegistered : IEvent, INotification
    {
        public InvestmentWasRegistered(string portfolioId, string investmentId, string investmentGroup, string investmentType, string currency, string symbol)
        {
            PortfolioId = portfolioId;
            InvestmentId = investmentId;
            InvestmentGroup = investmentGroup;
            InvestmentType = investmentType;
            Currency = currency;
            Symbol = symbol;
        }

        public string PortfolioId { get; }
        public string InvestmentId { get; }
        public string InvestmentType { get; }
        public string InvestmentGroup { get; }
        public string Symbol { get; }
        public string Currency { get; }
    }
}