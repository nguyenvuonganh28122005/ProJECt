using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "3")]
    public class N6HomeController : Controller
    {
        private readonly PjNetContext _context;

        public N6HomeController(PjNetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê số lượng tổng quan cho các thẻ đầu trang
            ViewBag.TotalThuoc = await _context.Thuocs.CountAsync();
            ViewBag.TotalNhomThuoc = await _context.NhomThuocs.CountAsync();
            ViewBag.TotalNhaCungCap = await _context.NhaCungCaps.CountAsync();
            ViewBag.TotalHoaDon = await _context.HoaDons.CountAsync();
            ViewBag.TotalTaiKhoan = await _context.TaiKhoans.CountAsync();
            ViewBag.TotalDoanhThu = await _context.HoaDons.Where(h => h.TrangThai != "DaHuy").SumAsync(h => h.TongTien ?? 0);
            ViewBag.ChoXacNhanRxCount = await _context.HoaDons.CountAsync(h => h.TrangThai == "ChoXacNhan" && h.LoaiDonTuVan != null);
            ViewBag.TotalLo = await _context.LoThuocs.CountAsync();

            // 1. Đơn hàng vừa tạo kèm trạng thái (lấy các đơn mới nhất)
            var recentOrders = await _context.HoaDons
                .Include(h => h.MaKhNavigation)
                .OrderByDescending(h => h.NgayLap)
                .Take(7)
                .ToListAsync();
            ViewBag.RecentOrders = recentOrders;

            // 2. Cảnh báo thuốc hết hạn & sắp hết hạn (trong vòng 60 ngày)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(60);

            var expiringLoThuoc = await _context.LoThuocs
                .Include(l => l.MaThuocNavigation)
                .Include(l => l.MaNccNavigation)
                .Where(l => l.HanSuDung <= warningDate)
                .OrderBy(l => l.HanSuDung)
                .Take(7)
                .ToListAsync();

            ViewBag.ExpiringLoThuoc = expiringLoThuoc;
            ViewBag.Today = today;
            ViewBag.LoHetHanCount = await _context.LoThuocs.CountAsync(l => l.HanSuDung < today);
            ViewBag.LoSapHetHanCount = await _context.LoThuocs.CountAsync(l => l.HanSuDung >= today && l.HanSuDung <= warningDate);

            return View();
        }
    }
}
