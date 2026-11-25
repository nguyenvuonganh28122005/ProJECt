using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Data;
using Staybook_sidePj.Models.Database;
using Staybook_sidePj.ViewModels;

namespace Staybook_sidePj.Controllers;

public sealed class AccountController(
    StaybookDbContext dbContext,
    IPasswordHasher<Nguoidung> passwordHasher) : Controller
{
    [HttpGet]
    public IActionResult Register(string? returnUrl = null) =>
        View(new CustomerRegisterViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CustomerRegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        if (await dbContext.Nguoidung.AnyAsync(user => user.Email == email, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
            return View(model);
        }

        var user = new Nguoidung
        {
            HoTen = model.Name.Trim(),
            Email = email,
            SoDt = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            TrangThaiHoatDong = true,
            VaiTro = "KhachHang"
        };
        user.MatKhauHash = passwordHasher.HashPassword(user, model.Password);
        dbContext.Nguoidung.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        await SignInCustomerAsync(user, cancellationToken);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Stays");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new CustomerLoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CustomerLoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Nguoidung.SingleOrDefaultAsync(
            item => item.Email == email && item.TrangThaiHoatDong,
            cancellationToken);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        PasswordVerificationResult verification;
        try
        {
            verification = passwordHasher.VerifyHashedPassword(user, user.MatKhauHash, model.Password);
        }
        catch (FormatException)
        {
            verification = PasswordVerificationResult.Failed;
        }
        if (verification == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.MatKhauHash = passwordHasher.HashPassword(user, model.Password);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await SignInCustomerAsync(user, cancellationToken);
        if (user.VaiTro == "QuanTriVien")
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Stays");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings(CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        if (customerId is null)
        {
            return Challenge();
        }

        var bookings = await dbContext.Datphong.AsNoTracking()
            .Where(booking => booking.MaNguoiDung == customerId.Value)
            .OrderByDescending(booking => booking.NgayTao)
            .ThenByDescending(booking => booking.MaDatPhong)
            .Select(booking => new CustomerBookingViewModel
            {
                Id = booking.MaDatPhong,
                AccommodationName = booking.MaNoiLuuTruNavigation.TenNoiLuuTru,
                CheckIn = booking.NgayNhanPhong,
                CheckOut = booking.NgayTraPhong,
                GuestCount = booking.SoLuongKhach,
                Total = booking.TongTien,
                Status = booking.TrangThaiDatPhong,
                RoomCount = booking.Chitietdatphong.Count
            })
            .ToListAsync(cancellationToken);

        return View(bookings);
    }

    private int? GetCustomerId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    private async Task SignInCustomerAsync(Nguoidung user, CancellationToken cancellationToken)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.MaNguoiDung.ToString()),
            new(ClaimTypes.Name, user.HoTen),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.VaiTro)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14) });
    }
}
