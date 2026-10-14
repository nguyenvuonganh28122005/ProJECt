using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.DuocSi.Controllers
{
    [Area("DuocSi")]
    [Authorize(Roles = "2,3")]
    public class BanHangController : Controller
    {
        private readonly PjNetContext _context;

        public BanHangController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /DuocSi/BanHang (UC07: Bán hàng tại quầy POS FEFO)
        public async Task<IActionResult> Index(string? search)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var query = _context.Thuocs
                .Include(t => t.MaNhomNavigation)
                .Include(t => t.LoThuocs)
                .Where(t => t.LoThuocs.Any(l => l.HanSuDung > today && l.SoLuongTon > 0))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.TenThuoc.Contains(search) 
                    || (t.HoatChat != null && t.HoatChat.Contains(search))
                    || t.LoThuocs.Any(l => l.SoLo.Contains(search)));
            }

            var medicines = await query
                .OrderBy(t => t.TenThuoc)
                .Take(30)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Today = today;

            return View(medicines);
        }

        // POST: /DuocSi/BanHang/LapHoaDon (Lập hóa đơn thanh toán trực tiếp tại quầy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LapHoaDon(List<ItemBanHangDto> items, string? hoTenKhach, string? sdtKhach, bool xacNhanDonBacSi)
        {
            if (items == null || !items.Any(i => i.SoLuong > 0))
            {
                TempData["ErrorMessage"] = "Hóa đơn chưa có sản phẩm nào.";
                return RedirectToAction(nameof(Index));
            }

            var validItems = items.Where(i => i.SoLuong > 0).ToList();
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Kiểm tra các lô thuốc
            var loIds = validItems.Select(i => i.MaLo).Distinct().ToList();
            var loThuocs = await _context.LoThuocs
                .Include(l => l.MaThuocNavigation)
                .Where(l => loIds.Contains(l.MaLo))
                .ToDictionaryAsync(l => l.MaLo);

            bool coThuocKeDon = false;
            decimal tongTien = 0;

            foreach (var item in validItems)
            {
                if (!loThuocs.TryGetValue(item.MaLo, out var lo))
                {
                    TempData["ErrorMessage"] = "Một số lô thuốc không tồn tại trong hệ thống.";
                    return RedirectToAction(nameof(Index));
                }

                // BR01: Chặn xuất lô đã hết hạn
                if (lo.HanSuDung <= today)
                {
                    TempData["ErrorMessage"] = $"Lô {lo.SoLo} của thuốc {lo.MaThuocNavigation?.TenThuoc} đã hết hạn sử dụng. Không thể bán!";
                    return RedirectToAction(nameof(Index));
                }

                // BR03: Ràng buộc tồn kho
                if (item.SoLuong > lo.SoLuongTon)
                {
                    TempData["ErrorMessage"] = $"Lô {lo.SoLo} của thuốc {lo.MaThuocNavigation?.TenThuoc} chỉ còn {lo.SoLuongTon} sản phẩm trong kho (yêu cầu {item.SoLuong}).";
                    return RedirectToAction(nameof(Index));
                }

                if (lo.MaThuocNavigation?.KeDon == true)
                {
                    coThuocKeDon = true;
                }

                tongTien += item.SoLuong * (lo.MaThuocNavigation?.GiaBan ?? 0);
            }

            // BR02: Nếu có thuốc kê đơn, Dược sĩ bắt buộc phải xác nhận có đơn Bác sĩ
            if (coThuocKeDon && !xacNhanDonBacSi)
            {
                TempData["ErrorMessage"] = "Hóa đơn có thuốc kê đơn (Rx). Dược sĩ bắt buộc phải kiểm tra và xác nhận có đơn Bác sĩ hợp lệ trước khi thanh toán!";
                return RedirectToAction(nameof(Index));
            }

            // Tạo Hóa Đơn Bán Tại Quầy
            var hoaDon = new HoaDon
            {
                MaKh = null, // Khách lẻ tại quầy
                NgayLap = DateTime.Now,
                TongTien = tongTien,
                HinhThuc = "TaiQuay",
                LoaiDonTuVan = coThuocKeDon ? "DonBacSiTaiQuay" : null,
                GhiChuKhachHang = $"Khách tại quầy: {(string.IsNullOrWhiteSpace(hoTenKhach) ? "Khách vãng lai" : hoTenKhach)} - SĐT: {sdtKhach}",
                TrangThai = "HoanTat" // Bán tại quầy thanh toán xong là hoàn tất
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            // Trừ tồn kho và lưu chi tiết hóa đơn
            foreach (var item in validItems)
            {
                var lo = loThuocs[item.MaLo];
                lo.SoLuongTon -= item.SoLuong; // Trừ kho

                var chiTiet = new ChiTietHoaDon
                {
                    MaHd = hoaDon.MaHd,
                    MaLo = lo.MaLo,
                    SoLuong = item.SoLuong,
                    DonGia = lo.MaThuocNavigation?.GiaBan ?? 0
                };
                _context.ChiTietHoaDons.Add(chiTiet);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thanh toán thành công hóa đơn #{hoaDon.MaHd}! Tổng tiền: {tongTien:N0} đ.";
            return RedirectToAction(nameof(InHoaDon), new { id = hoaDon.MaHd });
        }

        // GET: /DuocSi/BanHang/InHoaDon/5 (Xem phiếu in hóa đơn)
        public async Task<IActionResult> InHoaDon(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThuocNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }
    }

    public class ItemBanHangDto
    {
        public int MaLo { get; set; }
        public int SoLuong { get; set; }
    }
}
