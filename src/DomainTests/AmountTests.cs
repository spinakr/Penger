
using Domain.ValueObjects;
using FluentAssertions;

namespace DomainTests;

public class AmountTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Price_PositiveValie_ShouldConstruct()
    {
        var price = new Amount(101.1);
        price.Value.Should().Be(101.1);
    }

    [Test]
    public void Price_NegativeValues_ShouldThrow()
    {
        ((Action)(() =>
            new Amount(-1)
        )).Should().Throw<ArgumentException>();
    }
}