using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThuocController : Controller
    {
        private readonly PjNetContext _context;

        public ThuocController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/Thuoc
        public async Task<IActionResult> Index(string? search, int? maNhom)
        {
            var query = _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .AsQueryable();

            if (maNhom.HasValue && maNhom.Value > 0)
            {
                query = query.Where(t => t.MaNhom == maNhom.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.TenThuoc.Contains(search) 
                    || (t.HoatChat != null && t.HoatChat.Contains(search))
                    || (t.DangBaoChe != null && t.DangBaoChe.Contains(search)));
            }

            var list = await query.OrderBy(t => t.TenThuoc).ToListAsync();

            ViewBag.NhomThuocs = await _context.NhomThuocs.OrderBy(n => n.TenNhom).ToListAsync();
            ViewBag.CurrentMaNhom = maNhom;
            ViewBag.Search = search;

            return View(list);
        }
    }
}
