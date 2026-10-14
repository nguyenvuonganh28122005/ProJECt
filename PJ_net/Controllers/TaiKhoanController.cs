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

[AllowAnonymous]
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
    public IActionResult DangNhap(string? returnUrl = null)
    {
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

        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl!);

        return role switch
        {
            1 => RedirectToAction("Index", "Home"),
            2 => RedirectToAction("Index", "DuocSi"),
            _ => RedirectToAction("Index", "Home", new { area = "Admin" })
        };
    }

    [HttpGet]
    public IActionResult DangKy() => View(new RegisterViewModel());

    [HttpPost]
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
    public IActionResult QuenMK() => View(new ForgotPasswordViewModel());

    [HttpPost]
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
    public IActionResult DatLaiMK()
    {
        return HasValidResetSession() ? View(new ResetPasswordViewModel()) : RedirectToAction(nameof(QuenMK));
    }

    [HttpPost]
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
