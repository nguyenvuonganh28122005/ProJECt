using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Staybook_sidePj.Controllers;

[Route("Admin")]
[Authorize(Roles = "QuanTriVien")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect("/Admin/Dashboard");
    }
}
