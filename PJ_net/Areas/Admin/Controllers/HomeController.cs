using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly PjNetContext _context;

        public HomeController(PjNetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalThuoc = await _context.Thuocs.CountAsync();
            ViewBag.TotalNhomThuoc = await _context.NhomThuocs.CountAsync();
            ViewBag.TotalNhaCungCap = await _context.NhaCungCaps.CountAsync();
            ViewBag.TotalHoaDon = await _context.HoaDons.CountAsync();
            ViewBag.TotalTaiKhoan = await _context.TaiKhoans.CountAsync();

            return View();
        }
    }
}
