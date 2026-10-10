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

        // GET: /Admin/Thuoc (Danh sách mặt hàng thuốc)
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

        // GET: /Admin/Thuoc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thuoc = await _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                    .ThenInclude(l => l.MaNccNavigation)
                .FirstOrDefaultAsync(m => m.MaThuoc == id);

            if (thuoc == null)
            {
                return NotFound();
            }

            return View(thuoc);
        }

        // GET: /Admin/Thuoc/Create (Tạo mới mặt hàng thuốc vào danh mục)
        public async Task<IActionResult> Create()
        {
            ViewBag.NhomThuocs = await _context.NhomThuocs.OrderBy(n => n.TenNhom).ToListAsync();
            return View();
        }

        // POST: /Admin/Thuoc/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenThuoc,HoatChat,DangBaoChe,DonViTinh,GiaBan,KeDon,AnhThuoc,HuongDanSuDung,MaNhom")] Thuoc thuoc)
        {
            if (string.IsNullOrWhiteSpace(thuoc.TenThuoc))
            {
                ModelState.AddModelError("TenThuoc", "Tên thuốc không được để trống.");
            }

            if (thuoc.GiaBan < 0)
            {
                ModelState.AddModelError("GiaBan", "Giá bán không được nhỏ hơn 0.");
            }

            if (ModelState.IsValid)
            {
                thuoc.TenThuoc = thuoc.TenThuoc.Trim();
                thuoc.HoatChat = thuoc.HoatChat?.Trim();
                thuoc.DangBaoChe = thuoc.DangBaoChe?.Trim();
                thuoc.DonViTinh = thuoc.DonViTinh?.Trim();
                thuoc.HuongDanSuDung = thuoc.HuongDanSuDung?.Trim();

                _context.Add(thuoc);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã thêm mới mặt hàng thuốc \"{thuoc.TenThuoc}\" thành công! Bạn có thể nhập hàng lô thuốc ngay bây giờ.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.NhomThuocs = await _context.NhomThuocs.OrderBy(n => n.TenNhom).ToListAsync();
            return View(thuoc);
        }

        // GET: /Admin/Thuoc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thuoc = await _context.Thuocs.FindAsync(id);
            if (thuoc == null)
            {
                return NotFound();
            }

            ViewBag.NhomThuocs = await _context.NhomThuocs.OrderBy(n => n.TenNhom).ToListAsync();
            return View(thuoc);
        }

        // POST: /Admin/Thuoc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaThuoc,TenThuoc,HoatChat,DangBaoChe,DonViTinh,GiaBan,KeDon,AnhThuoc,HuongDanSuDung,MaNhom")] Thuoc thuoc)
        {
            if (id != thuoc.MaThuoc)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(thuoc.TenThuoc))
            {
                ModelState.AddModelError("TenThuoc", "Tên thuốc không được để trống.");
            }

            if (thuoc.GiaBan < 0)
            {
                ModelState.AddModelError("GiaBan", "Giá bán không được nhỏ hơn 0.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    thuoc.TenThuoc = thuoc.TenThuoc.Trim();
                    thuoc.HoatChat = thuoc.HoatChat?.Trim();
                    thuoc.DangBaoChe = thuoc.DangBaoChe?.Trim();
                    thuoc.DonViTinh = thuoc.DonViTinh?.Trim();
                    thuoc.HuongDanSuDung = thuoc.HuongDanSuDung?.Trim();

                    _context.Update(thuoc);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Cập nhật thuốc \"{thuoc.TenThuoc}\" thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Thuocs.AnyAsync(e => e.MaThuoc == thuoc.MaThuoc))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.NhomThuocs = await _context.NhomThuocs.OrderBy(n => n.TenNhom).ToListAsync();
            return View(thuoc);
        }

        // GET: /Admin/Thuoc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thuoc = await _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .FirstOrDefaultAsync(m => m.MaThuoc == id);

            if (thuoc == null)
            {
                return NotFound();
            }

            return View(thuoc);
        }

        // POST: /Admin/Thuoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var thuoc = await _context.Thuocs
                .Include(t => t.LoThuocs)
                .FirstOrDefaultAsync(m => m.MaThuoc == id);

            if (thuoc == null)
            {
                return NotFound();
            }

            if (thuoc.LoThuocs.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa thuốc \"{thuoc.TenThuoc}\" vì đã có {thuoc.LoThuocs.Count} lô thuốc trong kho liên kết với thuốc này.";
                return RedirectToAction(nameof(Index));
            }

            _context.Thuocs.Remove(thuoc);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa thuốc \"{thuoc.TenThuoc}\" khỏi danh mục.";
            return RedirectToAction(nameof(Index));
        }
    }
}
