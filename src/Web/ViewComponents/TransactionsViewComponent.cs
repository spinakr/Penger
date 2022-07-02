using Microsoft.AspNetCore.Mvc;

namespace Web.ViewComponents;

public class TransactionsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(List<Pages.Transactions.Index.Model.Transaction> transactions)
    {
        return View(transactions);
    }

}