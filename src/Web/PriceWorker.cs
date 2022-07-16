using System.Text.Json;
using Domain;
using Domain.Projections;
using Domain.ValueObjects;
using HtmlAgilityPack;
using MediatR;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;

namespace Web;

public class PriceWorker : BackgroundService
{
    private IEventStore _eventStore { get; set; }
    private IConfiguration _configuration { get; set; }
    private IMediator _mediator { get; set; }
    private IProjectionStore<string, List<RegisteredInvestmentsProjection>> _projectionStore { get; set; }

    public PriceWorker(IEventStore eventStore, IConfiguration configuration, IMediator mediator, IProjectionStore<string, List<RegisteredInvestmentsProjection>> projectionStore)
    {
        _eventStore = eventStore;
        _configuration = configuration;
        _mediator = mediator;
        _projectionStore = projectionStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        do
        {
            if (_configuration.GetValue<bool>("EnablePriceUpdates") == false) continue;
            var stream = _eventStore.LoadEventStream("kofoed");
            if (!stream.Events.Any()) continue;
            var portfolio = new Portfolio(stream.Events);
            var investments = _projectionStore.GetProjection("kofoed");

            HttpClient client = new HttpClient();
            var currencyString = await client.GetStringAsync(_configuration["CurrencyServiceUrl"]);
            var currencyJson = JsonSerializer.Deserialize<JsonElement>(currencyString);
            var usdToNok = 1 / currencyJson.GetProperty("rates").GetProperty("USD").GetDouble();
            var eurToNok = 1 / currencyJson.GetProperty("rates").GetProperty("EUR").GetDouble();

            System.Console.WriteLine("Updating prices...");
            foreach (var investment in investments)
            {
                var url = string.Format(_configuration.GetValue<string>("PriceServiceUrlTemplate"), investment.Symbol.Value);
                var html = await client.GetStringAsync(url);

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var priceNode = htmlDoc.DocumentNode.Descendants("td")
                    .Where(node => node.GetAttributeValue("data-test", "") == "PREV_CLOSE-value" ||
                                   node.GetAttributeValue("data-test", "") == "LAST_PRICE-value")
                    .First();

                var newPriceValue = decimal.Parse(priceNode.InnerText);
                var newPrice = new Money(newPriceValue, investment.Currency);
                var newNokPrice = investment.Currency.DisplayName switch
                {
                    "USD" => newPrice.ToNok(usdToNok),
                    "EUR" => newPrice.ToNok(eurToNok),
                    "NOK" => newPrice.ToNok(1),
                    _ => throw new InvalidDataException($"Unknown currency {investment.Currency.DisplayName}")
                };
                portfolio.UpdatePrice(investment.InvestmentId, newNokPrice, newPrice);

            }

            _eventStore.AppendToStream("kofoed", portfolio.PendingEvents, stream.Version);

        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}