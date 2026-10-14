using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class N6NhaCungCapController : Controller
    {
        private readonly PjNetContext _context;

        public N6NhaCungCapController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/NhaCungCap
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.NhaCungCaps
                .Include(n => n.LoThuocs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(n => n.TenNcc.Contains(search)
                    || (n.SoDienThoai != null && n.SoDienThoai.Contains(search))
                    || (n.DiaChi != null && n.DiaChi.Contains(search)));
            }

            var list = await query.OrderBy(n => n.TenNcc).ToListAsync();
            ViewBag.Search = search;
            return View(list);
        }
    }
}
