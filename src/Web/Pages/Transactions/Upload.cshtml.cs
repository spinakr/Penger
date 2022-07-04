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
        {"Gull (gram)", InvestmentGroup.Gold}
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
        {"Gull (gram)", InvestmentType.Commodity}
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
                        PortfolioId = "kofoed"
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
                });

            }
        }
        return new OkResult();
    }

}