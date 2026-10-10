using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;

namespace PJ_net.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanVienController : Controller
    {
        private readonly PjNetContext _context;

        public NhanVienController(PjNetContext context)
        {
            _context = context;
        }

        // GET: /Admin/NhanVien (Danh sách nhân viên / nhân sự nội bộ, VaiTro != 1)
        public async Task<IActionResult> Index(string? search, int? vaiTro)
        {
            // VaiTro != 1 (1 là khách hàng, các vai trò khác: 0: Admin, 2: Dược sĩ/Bán hàng, 3: Kho)
            var query = _context.TaiKhoans
                .Where(t => t.VaiTro != 1)
                .AsQueryable();

            if (vaiTro.HasValue)
            {
                query = query.Where(t => t.VaiTro == vaiTro.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(t => t.HoTen.Contains(search)
                    || t.TenDangNhap.Contains(search)
                    || (t.SoDienThoai != null && t.SoDienThoai.Contains(search)));
            }

            var list = await query.OrderBy(t => t.VaiTro).ThenBy(t => t.HoTen).ToListAsync();

            ViewBag.Search = search;
            ViewBag.CurrentVaiTro = vaiTro;

            return View(list);
        }

        // GET: /Admin/NhanVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.TaiKhoans
                .Include(t => t.HoaDons)
                .FirstOrDefaultAsync(m => m.MaTk == id && m.VaiTro != 1);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // GET: /Admin/NhanVien/Create
        public IActionResult Create()
        {
            var model = new TaiKhoan
            {
                VaiTro = 2 // Mặc định là Dược sĩ / Bán hàng
            };
            return View(model);
        }

        // POST: /Admin/NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,HoTen,SoDienThoai,VaiTro")] TaiKhoan nhanVien)
        {
            if (string.IsNullOrWhiteSpace(nhanVien.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập không được để trống.");
            }
            else
            {
                var usernameExists = await _context.TaiKhoans
                    .AnyAsync(t => t.TenDangNhap.ToLower() == nhanVien.TenDangNhap.Trim().ToLower());
                if (usernameExists)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập này đã được sử dụng.");
                }
            }

            if (string.IsNullOrWhiteSpace(nhanVien.MatKhau))
            {
                ModelState.AddModelError("MatKhau", "Mật khẩu không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(nhanVien.HoTen))
            {
                ModelState.AddModelError("HoTen", "Họ và tên không được để trống.");
            }

            if (nhanVien.VaiTro == 1)
            {
                ModelState.AddModelError("VaiTro", "Vai trò này là của Khách hàng, vui lòng chọn vai trò nhân sự nội bộ.");
            }

            if (ModelState.IsValid)
            {
                nhanVien.TenDangNhap = nhanVien.TenDangNhap.Trim();
                nhanVien.MatKhau = nhanVien.MatKhau.Trim();
                nhanVien.HoTen = nhanVien.HoTen.Trim();
                nhanVien.SoDienThoai = nhanVien.SoDienThoai?.Trim();
                nhanVien.VaiTro = nhanVien.VaiTro ?? 2;

                _context.Add(nhanVien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Thêm mới nhân viên \"{nhanVien.HoTen}\" thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(nhanVien);
        }

        // GET: /Admin/NhanVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.TaiKhoans
                .FirstOrDefaultAsync(m => m.MaTk == id && m.VaiTro != 1);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: /Admin/NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string hoTen, string? soDienThoai, int vaiTro, string? matKhauMoi)
        {
            var nhanVien = await _context.TaiKhoans
                .FirstOrDefaultAsync(m => m.MaTk == id && m.VaiTro != 1);

            if (nhanVien == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                ModelState.AddModelError("HoTen", "Họ và tên không được để trống.");
            }

            if (vaiTro == 1)
            {
                ModelState.AddModelError("VaiTro", "Không thể chuyển vai trò nhân viên thành Khách hàng.");
            }

            if (ModelState.IsValid)
            {
                nhanVien.HoTen = hoTen.Trim();
                nhanVien.SoDienThoai = soDienThoai?.Trim();
                nhanVien.VaiTro = vaiTro;

                // Nếu có nhập mật khẩu mới thì cập nhật
                if (!string.IsNullOrWhiteSpace(matKhauMoi))
                {
                    nhanVien.MatKhau = matKhauMoi.Trim();
                }

                _context.Update(nhanVien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Cập nhật thông tin nhân viên \"{nhanVien.HoTen}\" thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(nhanVien);
        }

        // GET: /Admin/NhanVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nhanVien = await _context.TaiKhoans
                .Include(t => t.HoaDons)
                .FirstOrDefaultAsync(m => m.MaTk == id && m.VaiTro != 1);

            if (nhanVien == null)
            {
                return NotFound();
            }

            return View(nhanVien);
        }

        // POST: /Admin/NhanVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.TaiKhoans
                .Include(t => t.HoaDons)
                .FirstOrDefaultAsync(m => m.MaTk == id && m.VaiTro != 1);

            if (nhanVien == null)
            {
                return NotFound();
            }

            // Kiểm tra ràng buộc dữ liệu hóa đơn
            if (nhanVien.HoaDons.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa nhân viên \"{nhanVien.HoTen}\" vì đã có dữ liệu hóa đơn liên quan. Bạn có thể đổi mật khẩu hoặc điều chỉnh vai trò.";
                return RedirectToAction(nameof(Index));
            }

            _context.TaiKhoans.Remove(nhanVien);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa nhân viên \"{nhanVien.HoTen}\" khỏi hệ thống.";
            return RedirectToAction(nameof(Index));
        }
    }
}
