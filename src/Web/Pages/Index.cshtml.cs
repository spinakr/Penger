using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class Index : PageModel
{
    private readonly ILogger<Index> _logger;

    public Index(ILogger<Index> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}
