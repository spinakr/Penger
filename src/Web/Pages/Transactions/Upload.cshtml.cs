using System.Text;
using Domain;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketCqrs.EventStore;

namespace Web.Pages.Transactions;

public class Upload : PageModel
{
    private readonly IMediator _messaging;

    public Upload(IMediator messaging) => _messaging = messaging;

    [BindProperty]
    public IFormFile FormFile { get; set; }

    private Dictionary<string, InvestmentGroup> _investmentGroups = new Dictionary<string, InvestmentGroup>
    {
        {"DNB Global Indeks", InvestmentGroup.GlobalIndex},
        {"iShs Autom&Robotics ETF USD A", InvestmentGroup.TechETF},
        {"KLP Aksje Fremvoksende Markeder Indeks II", InvestmentGroup.EmergingMarkets},
        {"KLP AksjeGlobal Mer Samfunnsansvar", InvestmentGroup.GlobalIndex},
        {"VanEck VecVidG&eSUCITSETFUSDA", InvestmentGroup.TechETF},
        {"Desert Control AS", InvestmentGroup.SingleStocks},
        {"Etherum", InvestmentGroup.Cryptocurrency},
        {"Bitcoin", InvestmentGroup.Cryptocurrency},
        {"Cardano", InvestmentGroup.Cryptocurrency},
        {"Ergo", InvestmentGroup.Cryptocurrency},
        {"Nano", InvestmentGroup.Cryptocurrency},
        {"Algorand", InvestmentGroup.Cryptocurrency},
        {"iShs Glbl Clean Engy ETF", InvestmentGroup.TechETF},
        {"iS S&P 500", InvestmentGroup.TechETF},
        {"VanEck Gold Miners UETF USD A", InvestmentGroup.Gold},
        {"Gull", InvestmentGroup.Gold}
    };

    private Dictionary<string, InvestmentType> _investmentTypes = new Dictionary<string, InvestmentType>
    {
        {"DNB Global Indeks", InvestmentType.Fund},
        {"iShs Autom&Robotics ETF USD A", InvestmentType.ETF},
        {"KLP Aksje Fremvoksende Markeder Indeks II", InvestmentType.Fund},
        {"KLP AksjeGlobal Mer Samfunnsansvar", InvestmentType.Fund},
        {"VanEck VecVidG&eSUCITSETFUSDA", InvestmentType.ETF},
        {"Etherum", InvestmentType.Cryptocurrency},
        {"Desert Control AS", InvestmentType.Stock},
        {"Bitcoin", InvestmentType.Cryptocurrency},
        {"Cardano", InvestmentType.Cryptocurrency},
        {"Ergo", InvestmentType.Cryptocurrency},
        {"Nano", InvestmentType.Cryptocurrency},
        {"Algorand", InvestmentType.Cryptocurrency},
        {"iShs Glbl Clean Engy ETF", InvestmentType.ETF},
        {"iS S&P 500", InvestmentType.ETF},
        {"VanEck Gold Miners UETF USD A", InvestmentType.Commodity},
        {"Gull", InvestmentType.Commodity}
    };

    private Dictionary<string, string> _investmentSymbols = new Dictionary<string, string>
    {
        {"DNB Global Indeks", "0P0000PS3V.IR"},
        {"iShs Autom&Robotics ETF USD A", "2B76.DE"},
        {"KLP Aksje Fremvoksende Markeder Indeks II", "0P0000TJ5D.IR"},
        {"KLP AksjeGlobal Mer Samfunnsansvar", "0P0001DFBS.IR"},
        {"VanEck VecVidG&eSUCITSETFUSDA", "ESP0.DE"},
        {"Etherum", "ETH-USD"},
        {"Desert Control AS", "DSRT.OL"},
        {"Bitcoin", "BTC-USD"},
        {"Cardano", "ADA-USD"},
        {"Ergo", "ERG-USD"},
        {"Nano", "XNO-USD"},
        {"Algorand", "ALGO-USD"},
        {"iShs Glbl Clean Engy ETF", "IQQH.DE"},
        {"iS S&P 500", "QDVE.DE"},
        {"VanEck Gold Miners UETF USD A", "G2X.DE"},
        {"Gull", "GC=F"}
    };

    private Dictionary<string, string> _investmentCurrency = new Dictionary<string, string>
    {
        {"DNB Global Indeks", "NOK"},
        {"iShs Autom&Robotics ETF USD A", "EUR"},
        {"KLP Aksje Fremvoksende Markeder Indeks II", "NOK"},
        {"KLP AksjeGlobal Mer Samfunnsansvar", "NOK"},
        {"VanEck VecVidG&eSUCITSETFUSDA", "EUR"},
        {"Etherum", "USD"},
        {"Desert Control AS", "NOK"},
        {"Bitcoin", "USD"},
        {"Cardano", "USD"},
        {"Ergo", "USD"},
        {"Nano", "USD"},
        {"Algorand", "USD"},
        {"iShs Glbl Clean Engy ETF", "EUR"},
        {"iS S&P 500", "EUR"},
        {"VanEck Gold Miners UETF USD A", "EUR"},
        {"Gull", "USD"}
    };

    public async Task<IActionResult> OnPostUploadAsync()
    {
        using (var reader = new StreamReader(FormFile.OpenReadStream()))
        {
            await reader.ReadLineAsync(); // skip header
            while (reader.Peek() >= 0)
            {
                var line = await reader.ReadLineAsync();
                if (line is null) continue;

                Transaction transaction;
                try
                {
                    transaction = Transaction.ParseFromCSVLine(line);
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(line);
                    throw ex;
                }
                var investmentGroup = _investmentGroups[transaction.InvestmentId.ToString()].DisplayName;
                var investmentType = _investmentTypes[transaction.InvestmentId.ToString()].DisplayName;
                try
                {
                    await _messaging.Send(new Investments.Create.Command
                    {
                        InvestmentId = transaction.InvestmentId.ToString(),
                        InvestmentGroup = investmentGroup,
                        InvestmentType = investmentType,
                        PortfolioId = "kofoed",
                        Symbol = _investmentSymbols[transaction.InvestmentId.ToString()],
                        Currency = _investmentCurrency[transaction.InvestmentId.ToString()]
                    });
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }

                await _messaging.Send(new Transactions.Create.Command
                {
                    PortfolioId = "kofoed",
                    InvestmentId = transaction.InvestmentId.ToString(),
                    Date = transaction.Date,
                    Amount = transaction.Amount,
                    Price = transaction.Price.Value,
                    Currency = transaction.Price.Currency.DisplayName,
                    Type = transaction.Type.DisplayName
                });

            }
        }
        return new OkResult();
    }

}