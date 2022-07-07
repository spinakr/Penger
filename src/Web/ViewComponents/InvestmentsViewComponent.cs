using Microsoft.AspNetCore.Mvc;
using Web.Projections;

namespace Web.ViewComponents;

public class InvestmentsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(List<RegisteredInvestment> investments)
    {
        return View(investments);
    }

}