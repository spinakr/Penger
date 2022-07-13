using MediatR;
using PocketCqrs;

namespace Domain.Events;

public class InvestmentPriceWasUpdated : IEvent, INotification
{
    public InvestmentPriceWasUpdated(string portfolioId, string investmentId,
        decimal nokPrice, decimal price, DateTime date)
    {
        PortfolioId = portfolioId;
        InvestmentId = investmentId;
        NokPrice = nokPrice;
        Date = date;
        Price = price;
    }

    public string PortfolioId { get; }
    public string InvestmentId { get; }
    public decimal NokPrice { get; }
    public decimal Price { get; }
    public DateTime Date { get; }
}