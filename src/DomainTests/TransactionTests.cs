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
        port.RegisterInvestment(new Investment("Gold", InvestmentType.Commodity.DisplayName, InvestmentGroup.Gold.DisplayName));
        port.AddTransaction(
            Transaction.CreateNew(new InvestmentId("Gold"),
            DateTime.Now,
            amount: 100,
            price: new decimal(50.5),
            fee: 5,
            currency: "NOK"
        ));

        port.Transactions.Count.Should().Be(1);
    }
}