using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Controllers
{
    public class N6DonHangController : Controller
    {
        private readonly PjNetContext _context;

        public N6DonHangController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /DonHang/LichSu (UC05: Lịch sử mua hàng & Tra cứu đơn)
        public async Task<IActionResult> LichSu(string? sdt, int? maHd)
        {
            var query = _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThuocNavigation)
                .AsQueryable();

            if (User.Identity?.IsAuthenticated == true)
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int maTk))
                {
                    query = query.Where(h => h.MaKh == maTk);
                }
            }
            else
            {
                // Khách chưa đăng nhập: yêu cầu nhập SĐT hoặc Mã đơn
                if (maHd.HasValue)
                {
                    query = query.Where(h => h.MaHd == maHd.Value);
                }
                else if (!string.IsNullOrWhiteSpace(sdt))
                {
                    sdt = sdt.Trim();
                    query = query.Where(h => h.GhiChuKhachHang != null && h.GhiChuKhachHang.Contains(sdt));
                }
                else
                {
                    // Trả về view rỗng để khách nhập tra cứu
                    ViewBag.RequiresSearch = true;
                    return View(new List<HoaDon>());
                }
            }

            var list = await query.OrderByDescending(h => h.NgayLap).ToListAsync();
            ViewBag.SearchSdt = sdt;
            ViewBag.SearchMaHd = maHd;

            return View(list);
        }

        // GET: /DonHang/ChiTiet/5 (Chi tiết đơn hàng)
        public async Task<IActionResult> ChiTiet(int? id)
        {
            if (id == null) return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThuocNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }
    }
}
