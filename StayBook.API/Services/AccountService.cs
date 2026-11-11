using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StayBook.API.Data;
using StayBook.API.DTOs;
using StayBook.API.Models;

namespace StayBook.API.Services;

public sealed class AccountService
{
    private readonly StayBookDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<NguoiDung> _passwordHasher = new();

    public AccountService(StayBookDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        if (await _dbContext.NguoiDungs.AnyAsync(x => x.Email == email, cancellationToken))
        {
            return null;
        }

        var user = new NguoiDung
        {
            HoTen = request.FullName.Trim(),
            Email = email,
            SoDT = request.PhoneNumber.Trim(),
            VaiTro = "KhachHang"
        };
        user.MatKhauHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.NguoiDungs.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.NguoiDungs.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null || !user.TrangThaiHoatDong)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.MatKhauHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.MatKhauHash = _passwordHasher.HashPassword(user, request.Password);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(NguoiDung user)
    {
        var section = _configuration.GetSection("Jwt");
        var key = section["Key"]!;
        var issuer = section["Issuer"]!;
        var audience = section["Audience"]!;
        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(section["ExpiresInMinutes"]!));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim(ClaimTypes.MobilePhone, user.SoDT),
            new Claim(ClaimTypes.Role, user.VaiTro)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt,
            new UserResponse(user.Id, user.HoTen, user.Email, user.SoDT, user.VaiTro));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
