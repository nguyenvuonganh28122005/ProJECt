using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PJ_net.Controllers;

[Authorize(Roles = "2")]
public class DuocSiController : Controller
{
    public IActionResult Index() => View();
}
