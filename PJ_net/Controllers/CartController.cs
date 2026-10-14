using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;
using PJ_net.ViewModels;

namespace PJ_net.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";
        private readonly PjNetContext _context;
        private readonly IWebHostEnvironment _env;

        public CartController(PjNetContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Cart (Xem giỏ hàng)
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int maThuoc, int soLuong = 1)
        {
            if (soLuong <= 0) soLuong = 1;

            var thuoc = await _context.Thuocs
                .Include(t => t.LoThuocs)
                .FirstOrDefaultAsync(t => t.MaThuoc == maThuoc);

            if (thuoc == null) return NotFound();

            var today = DateOnly.FromDateTime(DateTime.Today);
            int tonKhaDung = thuoc.LoThuocs
                .Where(l => l.HanSuDung > today)
                .Sum(l => l.SoLuongTon);

            if (tonKhaDung <= 0)
            {
                TempData["ErrorMessage"] = $"Mặt hàng \"{thuoc.TenThuoc}\" hiện đã hết hàng.";
                return RedirectToAction("Details", "Thuoc", new { id = maThuoc });
            }

            var cart = GetCartFromSession();
            var existing = cart.Items.FirstOrDefault(i => i.MaThuoc == maThuoc);

            if (existing != null)
            {
                if (existing.SoLuong + soLuong > tonKhaDung)
                {
                    existing.SoLuong = tonKhaDung;
                    TempData["WarningMessage"] = $"Đã cập nhật số lượng đến mức tối đa còn trong kho ({tonKhaDung} {thuoc.DonViTinh}).";
                }
                else
                {
                    existing.SoLuong += soLuong;
                    TempData["SuccessMessage"] = $"Đã cập nhật số lượng \"{thuoc.TenThuoc}\" trong giỏ hàng.";
                }
            }
            else
            {
                int qtyToAdd = Math.Min(soLuong, tonKhaDung);
                cart.Items.Add(new CartItem
                {
                    MaThuoc = thuoc.MaThuoc,
                    TenThuoc = thuoc.TenThuoc,
                    GiaBan = thuoc.GiaBan,
                    DonViTinh = thuoc.DonViTinh,
                    AnhThuoc = thuoc.AnhThuoc,
                    KeDon = thuoc.KeDon ?? false,
                    SoLuong = qtyToAdd
                });
                TempData["SuccessMessage"] = $"Đã thêm \"{thuoc.TenThuoc}\" vào giỏ hàng.";
            }

            SaveCartToSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int maThuoc, int soLuong)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.MaThuoc == maThuoc);

            if (item != null)
            {
                if (soLuong <= 0)
                {
                    cart.Items.Remove(item);
                }
                else
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    int tonKhaDung = await _context.LoThuocs
                        .Where(l => l.MaThuoc == maThuoc && l.HanSuDung > today)
                        .SumAsync(l => l.SoLuongTon);

                    item.SoLuong = Math.Min(soLuong, tonKhaDung);
                }
                SaveCartToSession(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int maThuoc)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.MaThuoc == maThuoc);
            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCartToSession(cart);
                TempData["SuccessMessage"] = $"Đã xóa \"{item.TenThuoc}\" khỏi giỏ hàng.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout (Màn hình đặt hàng)
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CheckoutViewModel { Cart = cart };

            // Nếu khách đã đăng nhập, tự điền họ tên & số điện thoại
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

        // POST: /Cart/Checkout (Thanh toán & Xuất kho theo FEFO BR01, BR02, BR03)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model, IFormFile? anhDonThuoc)
        {
            var cart = GetCartFromSession();
            model.Cart = cart;

            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction(nameof(Index));
            }

            // 1. KIỂM TRA RÀNG BUỘC THUỐC KÊ ĐƠN (BR02)
            if (cart.CoThuocKeDon)
            {
                if (string.IsNullOrWhiteSpace(model.LoaiDonTuVan))
                {
                    ModelState.AddModelError(nameof(model.LoaiDonTuVan), "Giỏ hàng có thuốc kê đơn (Rx). Vui lòng chọn hình thức cung cấp đơn thuốc (Tải ảnh đơn hoặc Nhập mã đơn điện tử).");
                }
                else if (model.LoaiDonTuVan == "GuiAnh")
                {
                    if (anhDonThuoc == null || anhDonThuoc.Length == 0)
                    {
                        ModelState.AddModelError(string.Empty, "Vui lòng tải lên tệp ảnh đơn thuốc của Bác sĩ.");
                    }
                    else
                    {
                        var ext = Path.GetExtension(anhDonThuoc.FileName).ToLowerInvariant();
                        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
                        if (!allowed.Contains(ext))
                        {
                            ModelState.AddModelError(string.Empty, "Ảnh đơn thuốc phải có định dạng .jpg, .png hoặc .webp.");
                        }
                    }
                }
                else if (model.LoaiDonTuVan == "GuiMaDon")
                {
                    if (string.IsNullOrWhiteSpace(model.MaDonThuocDienTu))
                    {
                        ModelState.AddModelError(nameof(model.MaDonThuocDienTu), "Vui lòng nhập Mã đơn thuốc điện tử quốc gia (do bệnh viện/phòng khám cấp).");
                    }
                }
            }

            // 2. KIỂM TRA TỒN KHO KHẢ DỤNG TỪ CÁC LÔ CÒN HẠN (BR01, BR03)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var batchesToDeduct = new List<(LoThuoc lo, int soLuongXuat, decimal donGia)>();

            foreach (var item in cart.Items)
            {
                // Lấy các lô còn hạn, còn tồn kho, sắp xếp FEFO: hạn dùng gần nhất xuất trước
                var loThuocs = await _context.LoThuocs
                    .Where(l => l.MaThuoc == item.MaThuoc && l.HanSuDung > today && l.SoLuongTon > 0)
                    .OrderBy(l => l.HanSuDung)
                    .ToListAsync();

                int totalStock = loThuocs.Sum(l => l.SoLuongTon);
                if (totalStock < item.SoLuong)
                {
                    ModelState.AddModelError(string.Empty, $"Mặt hàng \"{item.TenThuoc}\" không đủ tồn kho khả dụng (yêu cầu {item.SoLuong}, kho còn {totalStock}).");
                    continue;
                }

                // Phân bổ số lượng cần xuất theo FEFO
                int remainingToDeduct = item.SoLuong;
                foreach (var lo in loThuocs)
                {
                    if (remainingToDeduct <= 0) break;

                    int take = Math.Min(lo.SoLuongTon, remainingToDeduct);
                    batchesToDeduct.Add((lo, take, item.GiaBan));
                    remainingToDeduct -= take;
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 3. XỬ LÝ UPLOAD ẢNH ĐƠN THUỐC NẾU CÓ
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

            // 4. LẤY MÃ KHÁCH HÀNG NẾU CÓ
            int? maKh = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idStr, out int parsedId))
                {
                    maKh = parsedId;
                }
            }

            // 5. TẠO HÓA ĐƠN MỚI
            var hoaDon = new HoaDon
            {
                MaKh = maKh,
                NgayLap = DateTime.Now,
                TongTien = cart.TongTien,
                HinhThuc = "TrucTuyen",
                LoaiDonTuVan = cart.CoThuocKeDon ? model.LoaiDonTuVan : null,
                AnhDonThuoc = savedImagePath,
                MaDonThuocDienTu = cart.CoThuocKeDon && model.LoaiDonTuVan == "GuiMaDon" ? model.MaDonThuocDienTu?.Trim() : null,
                GhiChuKhachHang = $"Người nhận: {model.HoTen} - SĐT: {model.SoDienThoai} - Đ/c: {model.DiaChi}. Ghi chú: {model.GhiChuKhachHang}",
                TrangThai = "ChoXacNhan"
            };

            _context.HoaDons.Add(hoaDon);
            await _context.SaveChangesAsync(); // Sinh MaHd

            // 6. TRỪ TỒN KHO LÔ THUỐC (FEFO) VÀ TẠO CHI TIẾT HÓA ĐƠN
            foreach (var item in batchesToDeduct)
            {
                item.lo.SoLuongTon -= item.soLuongXuat; // Trừ kho
                var chiTiet = new ChiTietHoaDon
                {
                    MaHd = hoaDon.MaHd,
                    MaLo = item.lo.MaLo,
                    SoLuong = item.soLuongXuat,
                    DonGia = item.donGia
                };
                _context.ChiTietHoaDons.Add(chiTiet);
            }

            await _context.SaveChangesAsync();

            // 7. XÓA SẠCH GIỎ HÀNG SESSION VÀ CHUYỂN HƯỚNG
            HttpContext.Session.Remove(CartSessionKey);
            TempData["SuccessOrderId"] = hoaDon.MaHd;

            return RedirectToAction(nameof(OrderSuccess), new { id = hoaDon.MaHd });
        }

        // GET: /Cart/OrderSuccess/5
        public async Task<IActionResult> OrderSuccess(int id)
        {
            var hoaDon = await _context.HoaDons
                .Include(h => h.ChiTietHoaDons)
                    .ThenInclude(ct => ct.MaLoNavigation)
                        .ThenInclude(l => l.MaThuocNavigation)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }

        // Helper Session methods
        private CartViewModel GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new CartViewModel();

            try
            {
                return JsonSerializer.Deserialize<CartViewModel>(json) ?? new CartViewModel();
            }
            catch
            {
                return new CartViewModel();
            }
        }

        private void SaveCartToSession(CartViewModel cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }
    }
}
