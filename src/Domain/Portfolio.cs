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
    private Dictionary<InvestmentId, DateTime> _lastInvestmentPriceUpdate = new Dictionary<InvestmentId, DateTime>();





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
            PortfolioId = Id,
            Distribution = distribution.ToDictionary(x => x.Key.DisplayName, x => x.Value.Fraction)
        });
    }

    public void RegisterInvestment(Investment investment)
    {
        if (RegisteredInvestments.Any(i => i.Id == investment.Id)) throw new InvalidDataException("The same investmentId cannot be registered multiple times");

        Append(new InvestmentWasRegistered(
            portfolioId: Id,
            investmentId: investment.Id.Value,
            investmentType: investment.Type.DisplayName,
            investmentGroup: investment.Group.DisplayName,
            symbol: investment.Symbol,
            currency: investment.Currency.DisplayName
        ));
    }

    public void UpdatePrice(InvestmentId investmentId, NokPrice nokPrice, Price price)
    {
        var investment = RegisteredInvestments.FirstOrDefault(i => i.Id == investmentId);
        if (investment is null) throw new InvalidDataException("Investment not found");
        if (investment.Currency != price.Currency) throw new InvalidDataException("Currency mismatch");

        var lastUpdate = _lastInvestmentPriceUpdate.TryGetValue(investmentId, out var lastUpdateTime) ? lastUpdateTime : DateTime.MinValue;
        if (lastUpdate.AddDays(1) > DateTime.Now) return;

        Append(new InvestmentPriceWasUpdated(
            portfolioId: Id,
            investmentId: investmentId.Value,
            newPrice: price.Value,
            newNokPrice: nokPrice.Value,
            date: DateTime.Now
        ));

    }

    public void AddTransaction(Transaction transaction)
    {
        if (!RegisteredInvestments.Any(i => i.Id == transaction.InvestmentId)) throw new InvalidOperationException("Transaction cannot be registered for an investement that is not registered first");

        Append(new NewTransactionWasCreated
        {
            PortfolioId = Id,
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
        RegisteredInvestments.Add(new Investment(@event.InvestmentId, @event.InvestmentType, @event.InvestmentGroup, @event.Symbol, @event.Currency));
    }

    public void When(InvestmentPriceWasUpdated @event)
    {
        if (!_lastInvestmentPriceUpdate.ContainsKey(new InvestmentId(@event.InvestmentId)))
        {
            _lastInvestmentPriceUpdate.Add(new InvestmentId(@event.InvestmentId), @event.Date);
        }
        else
        {
            _lastInvestmentPriceUpdate[new InvestmentId(@event.InvestmentId)] = @event.Date;
        }
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

    }
}
