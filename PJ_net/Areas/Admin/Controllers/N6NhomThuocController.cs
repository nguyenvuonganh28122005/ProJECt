using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class N6NhomThuocController : Controller
    {
        private readonly PjNetContext _context;

        public N6NhomThuocController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/NhomThuoc
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.NhomThuocs
                .Include(n => n.Thuocs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(n => n.TenNhom.Contains(search) || (n.MoTa != null && n.MoTa.Contains(search)));
            }

            var list = await query.OrderBy(n => n.TenNhom).ToListAsync();
            ViewBag.Search = search;
            return View(list);
        }

        // GET: /Admin/NhomThuoc/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/NhomThuoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenNhom,MoTa")] NhomThuoc nhomThuoc)
        {
            nhomThuoc.TenNhom = nhomThuoc.TenNhom?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(nhomThuoc.TenNhom))
                ModelState.AddModelError(nameof(nhomThuoc.TenNhom), "Tên nhóm thuốc không được để trống.");

            if (await _context.NhomThuocs.AnyAsync(n => n.TenNhom.ToLower() == nhomThuoc.TenNhom.ToLower()))
                ModelState.AddModelError(nameof(nhomThuoc.TenNhom), "Tên nhóm thuốc này đã tồn tại trong hệ thống.");

            if (!ModelState.IsValid)
                return View(nhomThuoc);

            nhomThuoc.MoTa = nhomThuoc.MoTa?.Trim();
            _context.Add(nhomThuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thêm mới nhóm thuốc \"{nhomThuoc.TenNhom}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var nhomThuoc = await _context.NhomThuocs.FindAsync(id);
            return nhomThuoc == null ? NotFound() : View(nhomThuoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNhom,TenNhom,MoTa")] NhomThuoc nhomThuoc)
        {
            if (id != nhomThuoc.MaNhom)
                return NotFound();

            nhomThuoc.TenNhom = nhomThuoc.TenNhom?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(nhomThuoc.TenNhom))
                ModelState.AddModelError(nameof(nhomThuoc.TenNhom), "Tên nhóm thuốc không được để trống.");

            if (await _context.NhomThuocs.AnyAsync(n =>
                    n.MaNhom != id && n.TenNhom.ToLower() == nhomThuoc.TenNhom.ToLower()))
                ModelState.AddModelError(nameof(nhomThuoc.TenNhom), "Tên nhóm thuốc này đã tồn tại trong hệ thống.");

            if (!ModelState.IsValid)
                return View(nhomThuoc);

            nhomThuoc.MoTa = nhomThuoc.MoTa?.Trim();
            _context.Update(nhomThuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật nhóm thuốc \"{nhomThuoc.TenNhom}\" thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var nhomThuoc = await _context.NhomThuocs
                .Include(n => n.Thuocs)
                .FirstOrDefaultAsync(n => n.MaNhom == id);

            return nhomThuoc == null ? NotFound() : View(nhomThuoc);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhomThuoc = await _context.NhomThuocs
                .Include(n => n.Thuocs)
                .FirstOrDefaultAsync(n => n.MaNhom == id);

            if (nhomThuoc == null)
                return NotFound();

            if (nhomThuoc.Thuocs.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa nhóm thuốc \"{nhomThuoc.TenNhom}\" vì vẫn còn thuốc thuộc nhóm này.";
                return RedirectToAction(nameof(Index));
            }

            _context.NhomThuocs.Remove(nhomThuoc);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa nhóm thuốc \"{nhomThuoc.TenNhom}\".";
            return RedirectToAction(nameof(Index));
        }
    }
}
