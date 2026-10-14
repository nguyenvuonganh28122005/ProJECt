using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Controllers
{
    public class TuVanViewModel
    {
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string LoaiDonTuVan { get; set; } = "GuiAnh"; // "GuiAnh" hoặc "GuiMaDon"
        public string? MaDonThuocDienTu { get; set; }
        public string? GhiChu { get; set; }
    }

    public class TuVanController : Controller
    {
        private readonly PjNetContext _context;
        private readonly IWebHostEnvironment _env;

        public TuVanController(PjNetContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /TuVan/GuiDonThuoc (UC04: Gửi đơn thuốc để được tư vấn)
        [HttpGet]
        public async Task<IActionResult> GuiDonThuoc()
        {
            var vm = new TuVanViewModel();
            if (User.Identity?.IsAuthenticated == true)
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int maTk))
                {
                    var user = await _context.TaiKhoans.FindAsync(maTk);
                    if (user != null)
                    {
                        vm.HoTen = user.HoTen;
                        vm.SoDienThoai = user.SoDienThoai ?? string.Empty;
                    }
                }
            }
            return View(vm);
        }

        // POST: /TuVan/GuiDonThuoc
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiDonThuoc(TuVanViewModel model, IFormFile? anhDonThuoc)
        {
            if (string.IsNullOrWhiteSpace(model.HoTen))
            {
                ModelState.AddModelError(nameof(model.HoTen), "Vui lòng nhập họ và tên.");
            }
            if (string.IsNullOrWhiteSpace(model.SoDienThoai))
            {
                ModelState.AddModelError(nameof(model.SoDienThoai), "Vui lòng nhập số điện thoại.");
            }

            if (model.LoaiDonTuVan == "GuiAnh")
            {
                if (anhDonThuoc == null || anhDonThuoc.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "Vui lòng tải lên ảnh chụp đơn thuốc của Bác sĩ.");
                }
            }
            else if (model.LoaiDonTuVan == "GuiMaDon")
            {
                if (string.IsNullOrWhiteSpace(model.MaDonThuocDienTu))
                {
                    ModelState.AddModelError(nameof(model.MaDonThuocDienTu), "Vui lòng nhập Mã đơn thuốc điện tử quốc gia.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? savedImagePath = null;
            if (anhDonThuoc != null && anhDonThuoc.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "donthuoc");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(anhDonThuoc.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await anhDonThuoc.CopyToAsync(stream);
                }
                savedImagePath = $"/uploads/donthuoc/{fileName}";
            }

            int? maKh = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int parsedId))
                {
                    maKh = parsedId;
                }
            }

            var hoaDon = new HoaDon
            {
                MaKh = maKh,
                NgayLap = DateTime.Now,
                TongTien = 0,
                HinhThuc = "TrucTuyen",
                LoaiDonTuVan = model.LoaiDonTuVan,
                AnhDonThuoc = savedImagePath,
                MaDonThuocDienTu = model.LoaiDonTuVan == "GuiMaDon" ? model.MaDonThuocDienTu?.Trim() : null,
                GhiChuKhachHang = $"Yêu cầu tư vấn: {model.HoTen} - SĐT: {model.SoDienThoai} - Đ/c: {model.DiaChi}. Triệu chứng/yêu cầu: {model.GhiChu}",
                TrangThai = "ChoXacNhan"
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Gửi đơn thuốc thành công! Mã yêu cầu tư vấn của bạn là #{hoaDon.MaHd}. Dược sĩ An Tâm sẽ liên hệ với bạn trong thời gian sớm nhất.";
            return RedirectToAction("ThanhCong", new { id = hoaDon.MaHd });
        }

        [HttpGet]
        public async Task<IActionResult> ThanhCong(int id)
        {
            var hoaDon = await _context.HoaDons.FindAsync(id);
            if (hoaDon == null) return NotFound();
            return View(hoaDon);
        }
    }
}
