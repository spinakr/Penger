using Microsoft.AspNetCore.Mvc;
using Domain.Projections;

namespace Web.ViewComponents;

public class InvestmentsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(List<RegisteredInvestmentsProjection> investments)
    {
        return View(investments);
    }

}