using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;

namespace PJ_net.Areas.DuocSi.Controllers
{
    [Area("DuocSi")]
    [Authorize(Roles = "2,3")]
    public class DonHangController : Controller
    {
        private readonly PjNetContext _context;

        public DonHangController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /DuocSi/DonHang (Danh sách đơn hàng trực tuyến & yêu cầu tư vấn)
        public async Task<IActionResult> Index(string? trangThai, string? search)
        {
            var query = _context.HoaDons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.ChiTietHoaDons)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(h => h.TrangThai == trangThai);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                if (int.TryParse(search, out int maHd))
                {
                    query = query.Where(h => h.MaHd == maHd || (h.GhiChuKhachHang != null && h.GhiChuKhachHang.Contains(search)));
                }
                else
                {
                    query = query.Where(h => (h.GhiChuKhachHang != null && h.GhiChuKhachHang.Contains(search))
                        || (h.MaKhNavigation != null && (h.MaKhNavigation.HoTen.Contains(search) || (h.MaKhNavigation.SoDienThoai != null && h.MaKhNavigation.SoDienThoai.Contains(search)))));
                }
            }

            var list = await query
                .OrderByDescending(h => h.NgayLap)
                .ToListAsync();

            ViewBag.CurrentTrangThai = trangThai;
            ViewBag.Search = search;
            ViewBag.ChoXacNhanCount = await _context.HoaDons.CountAsync(h => h.TrangThai == "ChoXacNhan");

            return View(list);
        }

        // GET: /DuocSi/DonHang/ThamDinh/5 (UC09: Thẩm định đơn thuốc khách gửi)
        public async Task<IActionResult> ThamDinh(int? id)
        {
            if (id == null) return NotFound();

            var hoaDon = await _context.HoaDons
                .Include(h => h.MaKhNavigation)
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThuocNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }

        // POST: /DuocSi/DonHang/CapNhatTrangThai (UC10: Xác nhận, cập nhật trạng thái đơn trực tuyến)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int maHd, string trangThaiMoi, string? ghiChuDuyet)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == maHd);

            if (hoaDon == null) return NotFound();

            var validStatuses = new[] { "ChoXacNhan", "DaXacNhan", "DangGiao", "HoanTat", "DaHuy" };
            if (!validStatuses.Contains(trangThaiMoi))
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(ThamDinh), new { id = maHd });
            }

            // Nếu hủy đơn: hoàn lại số lượng tồn cho các lô thuốc nếu đơn đã trừ tồn kho
            if (trangThaiMoi == "DaHuy" && hoaDon.TrangThai != "DaHuy")
            {
                foreach (var ct in hoaDon.ChiTietHoaDons)
                {
                    if (ct.MaLoNavigation != null)
                    {
                        ct.MaLoNavigation.SoLuongTon += ct.SoLuong; // Hoàn kho
                    }
                }
            }

            hoaDon.TrangThai = trangThaiMoi;
            if (!string.IsNullOrWhiteSpace(ghiChuDuyet))
            {
                hoaDon.GhiChuKhachHang += $" | [Dược sĩ {User.Identity?.Name}: {ghiChuDuyet.Trim()}]";
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn #{maHd} sang \"{trangThaiMoi}\" thành công!";
            return RedirectToAction(nameof(ThamDinh), new { id = maHd });
        }
    }
}
