using Microsoft.AspNetCore.Mvc;

namespace Staybook_sidePj.Controllers;

[Route("Admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect("/Admin/Dashboard");
    }
}
