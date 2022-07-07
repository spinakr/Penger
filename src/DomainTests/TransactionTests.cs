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
            Transaction.CreateNew(new InvestmentId("Gold"),
            DateTime.Now,
            amount: 100,
            price: new Price(50.5m, CurrencyType.NOK),
            fee: new Price(5, CurrencyType.NOK)
        ));

        port.Transactions.Count.Should().Be(1);
    }
}