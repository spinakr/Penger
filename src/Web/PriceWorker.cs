using System.Text.Json;
using Domain;
using Domain.ValueObjects;
using HtmlAgilityPack;
using PocketCqrs.EventStore;

namespace Web;

public class PriceWorker : BackgroundService
{
    private IEventStore _eventStore { get; set; }
    private IConfiguration _configuration { get; set; }

    public PriceWorker(IEventStore eventStore, IConfiguration configuration)
    {
        _configuration = configuration;
        _eventStore = eventStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromDays(1));

        do
        {
            var stream = _eventStore.LoadEventStream("kofoed");
            if (!stream.Events.Any()) continue;
            var portfolio = new Portfolio(stream.Events);
            HttpClient client = new HttpClient();

            var currencyString = await client.GetStringAsync(_configuration["CurrencyServiceUrl"]);
            //parse json string and get value of "rates"
            var currencyJson = JsonSerializer.Deserialize<JsonElement>(currencyString);
            var usdToNok = 1 / currencyJson.GetProperty("rates").GetProperty("USD").GetDouble();
            var eurToNok = 1 / currencyJson.GetProperty("rates").GetProperty("EUR").GetDouble();
            foreach (var investment in portfolio.RegisteredInvestments)
            {
                var url = string.Format(_configuration.GetValue<string>("PriceServiceUrlTemplate"), investment.Symbol);
                var html = await client.GetStringAsync(url);

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var priceNode = htmlDoc.DocumentNode.Descendants("td")
                    .Where(node => node.GetAttributeValue("data-test", "") == "PREV_CLOSE-value" ||
                                   node.GetAttributeValue("data-test", "") == "LAST_PRICE-value")
                    .First();

                var newPriceValue = decimal.Parse(priceNode.InnerText);
                var newPrice = new Price(newPriceValue, investment.Currency);
                var newNokPrice = investment.Currency.DisplayName switch
                {
                    "USD" => newPrice.ToNok(usdToNok),
                    "EUR" => newPrice.ToNok(eurToNok),
                    "NOK" => newPrice.ToNok(1),
                    _ => throw new InvalidDataException($"Unknown currency {investment.Currency.DisplayName}")
                };
                portfolio.UpdatePrice(investment.Id, newNokPrice, newPrice);

            } // end foreach

            _eventStore.AppendToStream("kofoed", portfolio.PendingEvents, stream.Version);

        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}