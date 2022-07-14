using Domain;
using Domain.ValueObjects;
using FluentAssertions;

namespace DomainTests;

public class TransactionTests
{
    [Test]
    public void Portfolio_CreateNewPortfolio_ReturnsValidPortfolio()
    {
        var port = Portfolio.CreateNew("TEST");
        port.RegisterInvestment(new Investment("Gold", InvestmentType.Commodity.DisplayName, InvestmentGroup.Gold.DisplayName, "Gold", "USD"));
        port.AddTransaction(
            new Transaction(new InvestmentId("Gold"),
            new TransactionId(),
            DateTime.Now,
            amount: new Amount(100),
            price: new Money(50.5m, CurrencyType.NOK),
            fee: new Money(5, CurrencyType.NOK)
        ));

        port.Transactions.Count.Should().Be(1);
    }
}