
using Domain.ValueObjects;
using FluentAssertions;

namespace DomainTests;

public class SymbolTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("ALGO-USD")]
    [TestCase("0P0000PS3V.IR")]
    [TestCase("2B76.DE")]
    [TestCase("GC=F")]
    [TestCase("DSRT.OL")]
    public void ConstructSymbol_ValidValue_ShouldConstruct(string symbol)
    {
        var s = new Symbol(symbol);
        s.Value.Should().Be(symbol);
    }

    [TestCase("; drop table")]
    [TestCase("123456789asdfghasd")]
    public void ConstructSymbol_InValidValue_ShouldThrow(string invalidString)
    {
        ((Action)(() =>
            new Symbol(invalidString)
        )).Should().Throw<ArgumentException>();
    }
}