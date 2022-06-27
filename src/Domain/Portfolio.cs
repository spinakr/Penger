using Domain.Events;
using Domain.ValueObjects;
using PocketCqrs;

namespace Domain;
public class Portfolio : EventSourcedAggregate
{
    public Portfolio() { }

    public Portfolio(IEnumerable<IEvent> events) : base(events) { }
    public Dictionary<InvestmentGroup, Percent> DesiredDistribution { get; private set; }
    public List<Transaction> Transactions { get; private set; }

    public static Portfolio CreateNew()
    {
        var @event = new PortfolioCreated
        {
            PortfolioId = Guid.NewGuid().ToString()
        };
        var newPortfolio = new Portfolio();
        newPortfolio.Append(@event);
        return newPortfolio;
    }

    public void ChangeDistribution(Dictionary<InvestmentGroup, Percent> distribution)
    {
        if (distribution.Values.Sum(v => v.Fraction) != 1) throw new InvalidDataException("Distribution must sum to exactly 100%");
        if (distribution.Keys.GroupBy(x => x.Value).Any(x => x.Count() > 1)) throw new InvalidDataException("Distribution can only have one of each investment");

        Append(new PortfolioDistributionWasChanged
        {
            Distribution = distribution.ToDictionary(x => x.Key.DisplayName, x => x.Value.Fraction)
        });
    }

    public void AddTransaction(Transaction transaction)
    {
        Append(new NewTransactionWasCreated
        {
            Date = transaction.Date,
            InvestmentName = transaction.Investment.Name,
            InvestmentType = transaction.Investment.Type.DisplayName,
            Amount = transaction.Amount,
            Fee = transaction.Fee,
            Currency = transaction.Currency
        });
    }



    //-------------------------------------------------
    // Event handlers, building the state of the object
    //-------------------------------------------------
    public void When(PortfolioCreated @event)
    {
        Id = @event.PortfolioId;
    }

    public void When(PortfolioDistributionWasChanged @event)
    {
        DesiredDistribution = @event.Distribution.ToDictionary(
            x => Enumeration.FromDisplayName<InvestmentGroup>(x.Key), x => new Percent(x.Value));
    }

    public void When(NewTransactionWasCreated @event)
    {
        if (Transactions is null) Transactions = new List<Transaction>();
        Transactions.Add(new Transaction(
            new Investment(@event.InvestmentName, Enumeration.FromDisplayName<InvestmentType>(@event.InvestmentType)),
            @event.Date,
            @event.Amount,
            @event.Price,
            @event.Fee,
            @event.Currency
        ));
    }
}
