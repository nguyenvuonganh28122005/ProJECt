using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CanhBaoController : Controller
    {
        private readonly PjNetContext _context;

        public CanhBaoController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/CanhBao (UC: Cảnh báo hết hạn, hết hàng)
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(60);

            // 1. Lô thuốc đã hết hạn
            var loHetHan = await _context.LoThuocs
                .Include(l => l.MaThuocNavigation)
                .Include(l => l.MaNccNavigation)
                .Where(l => l.HanSuDung < today)
                .OrderBy(l => l.HanSuDung)
                .ToListAsync();

            // 2. Lô thuốc sắp hết hạn (trong vòng 60 ngày)
            var loSapHetHan = await _context.LoThuocs
                .Include(l => l.MaThuocNavigation)
                .Include(l => l.MaNccNavigation)
                .Where(l => l.HanSuDung >= today && l.HanSuDung <= warningDate)
                .OrderBy(l => l.HanSuDung)
                .ToListAsync();

            // Lấy danh sách thuốc kèm các lô
            var allThuoc = await _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .ToListAsync();

            // 3. Thuốc đã hết hàng (tồn kho = 0)
            var thuocHetHang = allThuoc
                .Where(t => t.LoThuocs.Sum(l => l.SoLuongTon) == 0)
                .OrderBy(t => t.TenThuoc)
                .ToList();

            // 4. Thuốc sắp hết hàng (tồn kho > 0 và <= 10)
            var thuocSapHetHang = allThuoc
                .Where(t => t.LoThuocs.Sum(l => l.SoLuongTon) > 0 && t.LoThuocs.Sum(l => l.SoLuongTon) <= 10)
                .OrderBy(t => t.LoThuocs.Sum(l => l.SoLuongTon))
                .ToList();

            ViewBag.LoHetHan = loHetHan;
            ViewBag.LoSapHetHan = loSapHetHan;
            ViewBag.ThuocHetHang = thuocHetHang;
            ViewBag.ThuocSapHetHang = thuocSapHetHang;
            ViewBag.Today = today;

            return View();
        }
    }
}
