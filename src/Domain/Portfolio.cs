using PocketCqrs;

namespace Domain;
public class Portfolio : EventSourcedAggregate
{
    public Portfolio() { }

    public Portfolio(IEnumerable<IEvent> events) : base(events) { }

    //    public List<Investment> Investments { get; set; }

    public static Portfolio CreateNew()
    {
        var @event = new PortfolioCreated(Guid.NewGuid().ToString());
        var newPortfolio = new Portfolio();
        newPortfolio.Append(@event);
        return newPortfolio;
    }

    public void When(PortfolioCreated @event)
    {
        Id = @event.PortfolioId;
    }
}
