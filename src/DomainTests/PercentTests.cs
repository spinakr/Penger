using Domain.ValueObjects;
using FluentAssertions;

namespace DomainTests;

public class PercentTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ConstructedFromDecimal_ValueBetween0and1_StringValueWithPercentSign()
    {
        var percent = new Percent(0.1);
        percent.ToString().Should().Be("10%");
    }

    [Test]
    public void ConstructedFromDecimal_ValueGreatherThan1_ShouldThrow()
    {
        ((Action)(() =>
        new Percent(1.2))).Should().NotThrow<ArgumentException>();
    }

    [Test]
    public void ConstructedFromDecimal_ValueBetween0and1_CorrectPercentageValue()
    {
        var percent = new Percent(0.15);
        percent.PercentageValue.Should().Be(15);
    }

    [Test]
    public void ConstructedFromDecimal_ValueBetween0and1_CorrectFractionValue()
    {
        var percent = new Percent(0.15);
        percent.Fraction.Should().Be(0.15);
    }
}