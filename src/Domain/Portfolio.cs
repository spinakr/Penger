using Domain.Events;
using Domain.ValueObjects;
using PocketCqrs;

namespace Domain;
public class Portfolio : EventSourcedAggregate
{
    public Portfolio() { }

    public Portfolio(IEnumerable<IEvent> events) : base(events) { }
    public Dictionary<InvestmentGroup, Percent> DesiredDistribution { get; private set; }
    public List<Investment> RegisteredInvestments { get; set; } = new List<Investment>();
    public List<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public Dictionary<InvestmentId, Price> CurrentInvestmentPrices { get; set; } = new Dictionary<InvestmentId, Price>();
    public decimal TotalPortfolioValue => Transactions.GroupBy(x => x.InvestmentId).Sum(g => (decimal)g.Sum(x => x.Amount * (x.Type == TransactionType.Sale ? -1 : 1)) * CurrentInvestmentPrices[g.Key].Value);



    public static Portfolio CreateNew(string name)
    {
        var @event = new PortfolioCreated
        {
            PortfolioId = name
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

    public void RegisterInvestment(Investment investment)
    {
        if (RegisteredInvestments.Any(i => i.Id == investment.Id)) throw new InvalidDataException("The same investmentId cannot be registered multiple times");

        Append(new InvestmentWasRegistered
        {
            InvestmentId = investment.Id.Value,
            InvestmentGroup = investment.Group.DisplayName,
            InvestmentType = investment.Type.DisplayName
        });
    }

    public void AddTransaction(Transaction transaction)
    {
        if (!RegisteredInvestments.Any(i => i.Id == transaction.InvestmentId)) throw new InvalidOperationException("Transaction cannot be registered for an investement that is not registered first");

        Append(new NewTransactionWasCreated
        {
            Date = transaction.Date,
            InvestmentId = transaction.InvestmentId.Value,
            TransactionId = transaction.TransactionId.Value,
            TransactionType = transaction.Type.DisplayName,
            Amount = transaction.Amount,
            Price = transaction.Price.Value,
            Fee = transaction.Fee.Value,
            Currency = transaction.Price.Currency.DisplayName
        });
    }


    //-------------------------------------------------
    // Event handlers, building the state of the aggregate
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

    public void When(InvestmentWasRegistered @event)
    {
        RegisteredInvestments.Add(new Investment(@event.InvestmentId, @event.InvestmentType, @event.InvestmentGroup));
    }

    public void When(NewTransactionWasCreated @event)
    {
        Transactions.Add(new Transaction(
            new InvestmentId(@event.InvestmentId),
            new TransactionId(@event.TransactionId),
            @event.Date,
            @event.Amount,
            new Price(@event.Price, @event.Currency),
            new Price(@event.Fee, @event.Currency),
            Enumeration.FromDisplayName<TransactionType>(@event.TransactionType)
        ));

        var investmentId = new InvestmentId(@event.InvestmentId);
        var newPrice = new Price(@event.Price, Enumeration.FromDisplayName<CurrencyType>(@event.Currency));
        if (!CurrentInvestmentPrices.ContainsKey(investmentId)) CurrentInvestmentPrices.Add(investmentId, newPrice);
        else CurrentInvestmentPrices[investmentId] = newPrice;
    }
}
