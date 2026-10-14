using Microsoft.AspNetCore.Mvc;
using PJ_net.Models;
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using PJ_net.Data;

namespace PJ_net.Controllers
{
    public class HomeController : Controller
    {
        private readonly PjNetContext _context;

        public HomeController(PjNetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var categories = await _context.NhomThuocs
                .OrderBy(n => n.MaNhom)
                .Take(6)
                .ToListAsync();

            var featuredMedicines = await _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .Where(t => t.LoThuocs.Any(l => l.HanSuDung > today && l.SoLuongTon > 0))
                .OrderBy(t => t.TenThuoc)
                .Take(8)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.FeaturedMedicines = featuredMedicines;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
