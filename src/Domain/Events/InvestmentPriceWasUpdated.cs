using MediatR;
using PocketCqrs;

namespace Domain.Events;

public class InvestmentPriceWasUpdated : IEvent, INotification
{
    public InvestmentPriceWasUpdated(string portfolioId, string investmentId, decimal newNokPrice, decimal newPrice, DateTime date)
    {
        PortfolioId = portfolioId;
        InvestmentId = investmentId;
        NokPrice = newNokPrice;
        Date = date;
        Price = newPrice;
    }

    public string PortfolioId { get; set; }
    public string InvestmentId { get; set; }
    public decimal NokPrice { get; set; }
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
}