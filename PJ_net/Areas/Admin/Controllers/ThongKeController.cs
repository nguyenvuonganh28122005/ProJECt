using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    public class ThuocBanChayDto
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public string? DonViTinh { get; set; }
        public int TongSoLuongBan { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    [Area("Admin")]
    public class ThongKeController : Controller
    {
        private readonly PjNetContext _context;

        public ThongKeController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/ThongKe (UC: Thống kê doanh thu, thuốc bán chạy)
        public async Task<IActionResult> Index()
        {
            var validInvoices = await _context.HoaDons
                .Where(h => h.TrangThai != "DaHuy")
                .ToListAsync();

            // Tổng doanh thu
            decimal tongDoanhThu = validInvoices.Sum(h => h.TongTien ?? 0);
            int tongSoDon = validInvoices.Count;

            // Doanh thu theo hình thức
            var theoHinhThuc = validInvoices
                .GroupBy(h => h.HinhThuc ?? "Trực tiếp")
                .Select(g => new
                {
                    HinhThuc = g.Key,
                    SoLuong = g.Count(),
                    DoanhThu = g.Sum(h => h.TongTien ?? 0)
                })
                .ToList();

            // Thống kê thuốc bán chạy từ ChiTietHoaDon
            var chiTietList = await _context.ChiTietHoaDons
                .Include(ct => ct.MaHdNavigation)
                .Include(ct => ct.MaLoNavigation)
                    .ThenInclude(l => l.MaThuocNavigation)
                .Where(ct => ct.MaHdNavigation.TrangThai != "DaHuy" && ct.MaLoNavigation.MaThuocNavigation != null)
                .ToListAsync();

            var topThuocBanChay = chiTietList
                .GroupBy(ct => new 
                { 
                    ct.MaLoNavigation.MaThuocNavigation!.MaThuoc, 
                    ct.MaLoNavigation.MaThuocNavigation!.TenThuoc, 
                    ct.MaLoNavigation.MaThuocNavigation!.DonViTinh 
                })
                .Select(g => new ThuocBanChayDto
                {
                    MaThuoc = g.Key.MaThuoc,
                    TenThuoc = g.Key.TenThuoc,
                    DonViTinh = g.Key.DonViTinh,
                    TongSoLuongBan = g.Sum(ct => ct.SoLuong),
                    TongDoanhThu = g.Sum(ct => ct.SoLuong * ct.DonGia)
                })
                .OrderByDescending(x => x.TongSoLuongBan)
                .Take(10)
                .ToList();

            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.TongSoDon = tongSoDon;
            ViewBag.TheoHinhThuc = theoHinhThuc;
            ViewBag.TopThuoc = topThuocBanChay;

            return View();
        }
    }
}
