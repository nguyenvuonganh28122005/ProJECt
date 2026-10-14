using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.ViewModels;

namespace PJ_net.Controllers
{
    public class N6ThuocController : Controller
    {
        private readonly PjNetContext _context;
        private const int PageSize = 12;

        public N6ThuocController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Thuoc (UC02: Tra cứu thuốc, phân trang 12 sp/trang)
        public async Task<IActionResult> Index(string? search, int? maNhom, decimal? minGia, decimal? maxGia, int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.TenThuoc.Contains(search) 
                    || (t.HoatChat != null && t.HoatChat.Contains(search))
                    || (t.DangBaoChe != null && t.DangBaoChe.Contains(search)));
            }

            if (maNhom.HasValue && maNhom.Value > 0)
            {
                query = query.Where(t => t.MaNhom == maNhom.Value);
            }

            if (minGia.HasValue && minGia.Value >= 0)
            {
                query = query.Where(t => t.GiaBan >= minGia.Value);
            }

            if (maxGia.HasValue && maxGia.Value > 0)
            {
                query = query.Where(t => t.GiaBan <= maxGia.Value);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var today = DateOnly.FromDateTime(DateTime.Today);

            var items = await query
                .OrderBy(t => t.TenThuoc)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(t => new ThuocItemDto
                {
                    MaThuoc = t.MaThuoc,
                    TenThuoc = t.TenThuoc,
                    HoatChat = t.HoatChat,
                    DangBaoChe = t.DangBaoChe,
                    DonViTinh = t.DonViTinh,
                    GiaBan = t.GiaBan,
                    KeDon = t.KeDon ?? false,
                    AnhThuoc = t.AnhThuoc,
                    TenNhom = t.MaNhomNavigation != null ? t.MaNhomNavigation.TenNhom : null,
                    TongTonKhaDung = t.LoThuocs.Where(l => l.HanSuDung > today).Sum(l => l.SoLuongTon)
                })
                .ToListAsync();

            var vm = new ThuocListViewModel
            {
                Thuocs = items,
                NhomThuocs = await _context.NhomThuocs.OrderBy(n => n.TenNhom).ToListAsync(),
                Search = search,
                MaNhom = maNhom,
                MinGia = minGia,
                MaxGia = maxGia,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = PageSize
            };

            return View(vm);
        }

        // GET: /Thuoc/Details/5 (Chi tiết thuốc)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var thuoc = await _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .FirstOrDefaultAsync(m => m.MaThuoc == id);

            if (thuoc == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Today);
            int tonKhaDung = thuoc.LoThuocs
                .Where(l => l.HanSuDung > today)
                .Sum(l => l.SoLuongTon);

            var cungNhom = await _context.Thuocs
                .Where(t => t.MaNhom == thuoc.MaNhom && t.MaThuoc != thuoc.MaThuoc)
                .OrderBy(t => t.TenThuoc)
                .Take(4)
                .ToListAsync();

            var vm = new ThuocDetailViewModel
            {
                Thuoc = thuoc,
                TongTonKhaDung = tonKhaDung,
                ThuocCungNhom = cungNhom
            };

            return View(vm);
        }
    }
}
