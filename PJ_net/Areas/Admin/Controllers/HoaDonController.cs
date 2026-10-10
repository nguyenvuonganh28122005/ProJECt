using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HoaDonController : Controller
    {
        private readonly PjNetContext _context;

        public HoaDonController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/HoaDon
        public async Task<IActionResult> Index(string? search, string? trangThai)
        {
            var query = _context.HoaDons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.ChiTietHoaDons)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                if (int.TryParse(search, out int maHd))
                {
                    query = query.Where(h => h.MaHd == maHd || (h.MaKhNavigation != null && (h.MaKhNavigation.HoTen.Contains(search) || (h.MaKhNavigation.SoDienThoai != null && h.MaKhNavigation.SoDienThoai.Contains(search)))));
                }
                else
                {
                    query = query.Where(h => (h.MaKhNavigation != null && (h.MaKhNavigation.HoTen.Contains(search) || (h.MaKhNavigation.SoDienThoai != null && h.MaKhNavigation.SoDienThoai.Contains(search))))
                        || (h.HinhThuc != null && h.HinhThuc.Contains(search)));
                }
            }

            var list = await query.OrderByDescending(h => h.NgayLap).ToListAsync();
            ViewBag.Search = search;
            ViewBag.CurrentTrangThai = trangThai;

            return View(list);
        }

        // GET: /Admin/HoaDon/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThuocNavigation)
                .FirstOrDefaultAsync(m => m.MaHd == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            return View(hoaDon);
        }
    }
}
