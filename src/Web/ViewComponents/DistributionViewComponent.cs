using Microsoft.AspNetCore.Mvc;

namespace Web.ViewComponents;

public class DistributionViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(Dictionary<Domain.ValueObjects.InvestmentGroup, Domain.ValueObjects.Percent> distribution)
    {
        return View(distribution);
    }

}