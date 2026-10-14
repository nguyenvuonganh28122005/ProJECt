using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;

namespace PJ_net.Areas.DuocSi.Controllers
{
    [Area("DuocSi")]
    [Authorize(Roles = "2,3")]
    public class LoThuocController : Controller
    {
        private readonly PjNetContext _context;

        public LoThuocController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /DuocSi/LoThuoc (UC08: Tra cứu hàng tồn theo lô)
        public async Task<IActionResult> Index(string? search, string? filterHan)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(60);

            var query = _context.LoThuocs
                .Include(l => l.MaThuocNavigation)
                .Include(l => l.MaNccNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(l => l.SoLo.Contains(search)
                    || (l.MaThuocNavigation != null && (l.MaThuocNavigation.TenThuoc.Contains(search) || (l.MaThuocNavigation.HoatChat != null && l.MaThuocNavigation.HoatChat.Contains(search)))));
            }

            if (filterHan == "hethan")
            {
                query = query.Where(l => l.HanSuDung < today);
            }
            else if (filterHan == "canhan")
            {
                query = query.Where(l => l.HanSuDung >= today && l.HanSuDung <= warningDate);
            }
            else if (filterHan == "conhan")
            {
                query = query.Where(l => l.HanSuDung > warningDate);
            }

            // Sắp xếp FEFO: hạn dùng gần nhất lên đầu
            var list = await query
                .OrderBy(l => l.HanSuDung)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.FilterHan = filterHan;
            ViewBag.Today = today;
            ViewBag.WarningDate = warningDate;

            return View(list);
        }
    }
}
