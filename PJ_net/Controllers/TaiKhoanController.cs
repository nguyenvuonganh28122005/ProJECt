using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;
using PJ_net.Models;
using PJ_net.ViewModels;

namespace PJ_net.Controllers;

public class TaiKhoanController : Controller
{
    private const string ResetAccountSessionKey = "PasswordResetAccountId";
    private const string ResetExpirySessionKey = "PasswordResetExpiresAt";
    private readonly PjNetContext _context;
    private readonly PasswordHasher<TaiKhoan> _passwordHasher = new();

    public TaiKhoanController(PjNetContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        // Nếu đã đăng nhập trước đó, tự động phát hiện vai trò và chuyển hướng ngay về đúng phân hệ
        if (User.Identity?.IsAuthenticated == true)
        {
            return DieuHuongTheoVaiTro();
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var username = model.TenDangNhap.Trim();
        var account = await _context.TaiKhoans
            .FirstOrDefaultAsync(t => t.TenDangNhap.ToLower() == username.ToLower());

        if (account == null || !VerifyPassword(account, model.MatKhau, out var needsRehash))
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            return View(model);
        }

        if (needsRehash)
        {
            account.MatKhau = _passwordHasher.HashPassword(account, model.MatKhau);
            await _context.SaveChangesAsync();
        }

        var storedRole = account.VaiTro ?? 1;
        var role = storedRole == 0 ? 3 : storedRole;
        if (role is not (1 or 2 or 3))
        {
            ModelState.AddModelError(string.Empty, "Tài khoản chưa được gán vai trò hợp lệ.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.MaTk.ToString()),
            new(ClaimTypes.Name, account.HoTen),
            new(ClaimTypes.Role, role.ToString()),
            new("Username", account.TenDangNhap)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        // 1. Quản trị viên (Role 3): Chuyển thẳng về Bảng điều khiển Quản trị (/Admin/Home)
        if (role == 3)
        {
            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }

        // 2. Dược sĩ (Role 2): Chuyển thẳng về Màn hình Bán lẻ POS (/DuocSi/BanHang)
        if (role == 2)
        {
            return RedirectToAction("Index", "BanHang", new { area = "DuocSi" });
        }

        // 3. Khách hàng (Role 1): Nếu đang dở đơn hàng hoặc xem thuốc thì quay lại đúng trang, còn lại về Trang chủ
        if (Url.IsLocalUrl(returnUrl) &&
            !returnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) &&
            !returnUrl.StartsWith("/DuocSi", StringComparison.OrdinalIgnoreCase) &&
            !returnUrl.StartsWith("/TaiKhoan", StringComparison.OrdinalIgnoreCase) &&
            returnUrl != "/")
        {
            return Redirect(returnUrl!);
        }

        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpGet]
    [Authorize]
    public IActionResult DieuHuongTheoVaiTro()
    {
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);
        return roleClaim switch
        {
            "3" => RedirectToAction("Index", "Home", new { area = "Admin" }),
            "2" => RedirectToAction("Index", "BanHang", new { area = "DuocSi" }),
            _ => RedirectToAction("Index", "Home", new { area = "" })
        };
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult DangKy()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return DieuHuongTheoVaiTro();
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var username = model.TenDangNhap.Trim();
        if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap.ToLower() == username.ToLower()))
        {
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã được sử dụng.");
            return View(model);
        }

        var account = new TaiKhoan
        {
            TenDangNhap = username,
            HoTen = model.HoTen.Trim(),
            SoDienThoai = model.SoDienThoai.Trim(),
            VaiTro = 1
        };
        account.MatKhau = _passwordHasher.HashPassword(account, model.MatKhau);

        _context.TaiKhoans.Add(account);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.TenDangNhap), "Không thể tạo tài khoản. Tên đăng nhập có thể đã được sử dụng.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Đăng ký thành công. Hãy đăng nhập bằng tài khoản vừa tạo.";
        return RedirectToAction(nameof(DangNhap));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult QuenMK() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuenMK(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var username = model.TenDangNhap.Trim().ToLower();
        var fullName = model.HoTen.Trim().ToLower();
        var phone = model.SoDienThoai.Trim();
        var account = await _context.TaiKhoans.FirstOrDefaultAsync(t =>
            t.TenDangNhap.ToLower() == username &&
            t.HoTen.ToLower() == fullName &&
            t.SoDienThoai == phone);

        if (account == null)
        {
            ModelState.AddModelError(string.Empty, "Thông tin không khớp với tài khoản. Hãy kiểm tra lại hoặc liên hệ quản trị viên.");
            return View(model);
        }

        HttpContext.Session.SetInt32(ResetAccountSessionKey, account.MaTk);
        HttpContext.Session.SetString(
            ResetExpirySessionKey,
            DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds().ToString());
        return RedirectToAction(nameof(DatLaiMK));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult DatLaiMK()
    {
        return HasValidResetSession() ? View(new ResetPasswordViewModel()) : RedirectToAction(nameof(QuenMK));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatLaiMK(ResetPasswordViewModel model)
    {
        if (!HasValidResetSession())
            return RedirectToAction(nameof(QuenMK));

        if (!ModelState.IsValid)
            return View(model);

        var accountId = HttpContext.Session.GetInt32(ResetAccountSessionKey);
        var account = accountId.HasValue ? await _context.TaiKhoans.FindAsync(accountId.Value) : null;
        if (account == null)
            return RedirectToAction(nameof(QuenMK));

        account.MatKhau = _passwordHasher.HashPassword(account, model.MatKhau);
        await _context.SaveChangesAsync();
        HttpContext.Session.Remove(ResetAccountSessionKey);
        HttpContext.Session.Remove(ResetExpirySessionKey);

        TempData["SuccessMessage"] = "Đã đổi mật khẩu. Bạn có thể đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(DangNhap));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(DangNhap));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ThongTin()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out int maTk))
            return RedirectToAction(nameof(DangNhap));

        var user = await _context.TaiKhoans.FindAsync(maTk);
        if (user == null)
            return NotFound();

        var vm = new ProfileViewModel
        {
            TenDangNhap = user.TenDangNhap,
            HoTen = user.HoTen,
            SoDienThoai = user.SoDienThoai ?? string.Empty,
            VaiTroTen = user.VaiTro switch
            {
                3 => "Quản trị viên (Admin)",
                2 => "Dược sĩ bán hàng",
                _ => "Khách hàng thân thiết"
            }
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThongTin(ProfileViewModel model)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out int maTk))
            return RedirectToAction(nameof(DangNhap));

        var user = await _context.TaiKhoans.FindAsync(maTk);
        if (user == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.TenDangNhap = user.TenDangNhap;
            return View(model);
        }

        user.HoTen = model.HoTen.Trim();
        user.SoDienThoai = model.SoDienThoai.Trim();
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
        return RedirectToAction(nameof(ThongTin));
    }

    [HttpGet]
    [Authorize]
    public IActionResult DoiMatKhau() => View(new ChangePasswordViewModel());

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiMatKhau(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out int maTk))
            return RedirectToAction(nameof(DangNhap));

        var user = await _context.TaiKhoans.FindAsync(maTk);
        if (user == null)
            return RedirectToAction(nameof(DangNhap));

        if (!VerifyPassword(user, model.MatKhauHienTai, out _))
        {
            ModelState.AddModelError(nameof(model.MatKhauHienTai), "Mật khẩu hiện tại không đúng.");
            return View(model);
        }

        user.MatKhau = _passwordHasher.HashPassword(user, model.MatKhauMoi);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
        return RedirectToAction(nameof(ThongTin));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult TuChoi() => View();

    private bool VerifyPassword(TaiKhoan account, string suppliedPassword, out bool needsRehash)
    {
        needsRehash = false;
        if (account.MatKhau.StartsWith("AQAAAA", StringComparison.Ordinal))
        {
            try
            {
                var result = _passwordHasher.VerifyHashedPassword(account, account.MatKhau, suppliedPassword);
                if (result != PasswordVerificationResult.Failed)
                {
                    needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
                    return true;
                }
            }
            catch (FormatException)
            {
            }
        }

        if (!string.Equals(account.MatKhau, suppliedPassword, StringComparison.Ordinal))
            return false;

        needsRehash = true;
        return true;
    }

    private bool HasValidResetSession()
    {
        var accountId = HttpContext.Session.GetInt32(ResetAccountSessionKey);
        var expiryValue = HttpContext.Session.GetString(ResetExpirySessionKey);
        return accountId.HasValue &&
               long.TryParse(expiryValue, out var expiry) &&
               DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= expiry;
    }
}
