using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class N6KhachHangController : Controller
    {
        private readonly PjNetContext _context;

        public N6KhachHangController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/KhachHang (UC: Quản lý khách hàng)
        public async Task<IActionResult> Index(string? search)
        {
            // VaiTro == 1 là khách hàng
            var query = _context.TaiKhoans
                .Include(t => t.HoaDons)
                .Where(t => t.VaiTro == 1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.HoTen.Contains(search) 
                    || t.TenDangNhap.Contains(search) 
                    || (t.SoDienThoai != null && t.SoDienThoai.Contains(search)));
            }

            var list = await query.OrderBy(t => t.HoTen).ToListAsync();
            ViewBag.Search = search;
            return View(list);
        }

        // GET: /Admin/KhachHang/Details/5 (Lịch sử mua hàng của khách)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.TaiKhoans
                .Include(t => t.HoaDons)
                    .ThenInclude(h => h.ChiTietHoaDons)
                .FirstOrDefaultAsync(m => m.MaTk == id && m.VaiTro == 1);

            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }
    }
}
