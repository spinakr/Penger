
using Domain.ValueObjects;
using FluentAssertions;

namespace DomainTests;

public class PriceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Price_PositiveValie_ShouldConstruct()
    {
        var price = new Price(new Decimal(101.1), CurrencyType.NOK);
        price.Value.Should().Be(new decimal(101.1));
        price.Currency.Should().Be(CurrencyType.NOK);
    }

    [Test]
    public void Price_NullValue_ShouldThrow()
    {
        ((Action)(() =>
            new Price(-1, CurrencyType.NOK)
        )).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Price_CurrencyNull_ShouldThrow()
    {
        ((Action)(() =>
            new Price(1, (CurrencyType)null)
        )).Should().Throw<ArgumentException>();
    }
}