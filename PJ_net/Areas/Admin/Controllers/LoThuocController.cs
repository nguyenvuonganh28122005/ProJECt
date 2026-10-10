using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoThuocController : Controller
    {
        private readonly PjNetContext _context;

        public LoThuocController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/LoThuoc (Quản lý lô thuốc)
        public async Task<IActionResult> Index(string? search, int? maThuoc, string? filterHan)
        {
            var query = _context.LoThuocs
                .Include(l => l.MaThuocNavigation)
                .Include(l => l.MaNccNavigation)
                .AsQueryable();

            if (maThuoc.HasValue && maThuoc.Value > 0)
            {
                query = query.Where(l => l.MaThuoc == maThuoc.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(l => l.SoLo.Contains(search) 
                    || (l.MaThuocNavigation != null && l.MaThuocNavigation.TenThuoc.Contains(search))
                    || (l.MaNccNavigation != null && l.MaNccNavigation.TenNcc.Contains(search)));
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(60);

            if (filterHan == "hethan")
            {
                query = query.Where(l => l.HanSuDung < today);
            }
            else if (filterHan == "saphethan")
            {
                query = query.Where(l => l.HanSuDung >= today && l.HanSuDung <= warningDate);
            }
            else if (filterHan == "conhan")
            {
                query = query.Where(l => l.HanSuDung > warningDate);
            }

            var list = await query.OrderBy(l => l.HanSuDung).ToListAsync();

            ViewBag.Thuocs = await _context.Thuocs.OrderBy(t => t.TenThuoc).ToListAsync();
            ViewBag.CurrentMaThuoc = maThuoc;
            ViewBag.FilterHan = filterHan;
            ViewBag.Search = search;
            ViewBag.Today = today;

            return View(list);
        }

        // GET: /Admin/LoThuoc/Create (UC: Nhập hàng)
        public async Task<IActionResult> Create()
        {
            ViewBag.Thuocs = await _context.Thuocs.OrderBy(t => t.TenThuoc).ToListAsync();
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.OrderBy(n => n.TenNcc).ToListAsync();
            return View();
        }

        // POST: /Admin/LoThuoc/Create (UC: Nhập hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaThuoc,MaNcc,SoLo,NgaySx,HanSuDung,SoLuongTon,GiaNhap")] LoThuoc loThuoc)
        {
            if (string.IsNullOrWhiteSpace(loThuoc.SoLo))
            {
                ModelState.AddModelError("SoLo", "Số lô không được để trống.");
            }

            if (loThuoc.MaThuoc == null || loThuoc.MaThuoc <= 0)
            {
                ModelState.AddModelError("MaThuoc", "Vui lòng chọn thuốc cần nhập.");
            }

            if (loThuoc.MaNcc == null || loThuoc.MaNcc <= 0)
            {
                ModelState.AddModelError("MaNcc", "Vui lòng chọn nhà cung cấp.");
            }

            if (loThuoc.SoLuongTon <= 0)
            {
                ModelState.AddModelError("SoLuongTon", "Số lượng nhập phải lớn hơn 0.");
            }

            if (loThuoc.GiaNhap < 0)
            {
                ModelState.AddModelError("GiaNhap", "Giá nhập không hợp lệ.");
            }

            if (loThuoc.NgaySx.HasValue && loThuoc.HanSuDung <= loThuoc.NgaySx.Value)
            {
                ModelState.AddModelError("HanSuDung", "Hạn sử dụng phải sau ngày sản xuất.");
            }

            if (ModelState.IsValid)
            {
                loThuoc.SoLo = loThuoc.SoLo.Trim();
                _context.Add(loThuoc);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Nhập hàng thành công! Đã thêm lô #{loThuoc.SoLo} vào kho.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Thuocs = await _context.Thuocs.OrderBy(t => t.TenThuoc).ToListAsync();
            ViewBag.NhaCungCaps = await _context.NhaCungCaps.OrderBy(n => n.TenNcc).ToListAsync();
            return View(loThuoc);
        }
    }
}
