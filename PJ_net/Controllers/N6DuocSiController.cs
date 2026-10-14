using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PJ_net.Controllers;

[Authorize(Roles = "2,3")]
public class N6DuocSiController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "BanHang", new { area = "DuocSi" });
}
