using Microsoft.AspNetCore.Mvc;

namespace Web.ViewComponents;

public class InvestmentsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(List<Pages.Investments.Index.Model.Investment> investments)
    {
        return View(investments);
    }

}