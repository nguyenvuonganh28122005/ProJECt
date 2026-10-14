using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using PJ_net.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Conventions.Add(new PJ_net.N6ControllerModelConvention());
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new PJ_net.N6ViewLocationExpander());
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "PJ_net.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.AccessDeniedPath = "/TaiKhoan/TuChoi";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(10);
});

// Configure DbContext with SQL Server
builder.Services.AddDbContext<PjNetContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapStaticAssets();

// Tự động chuyển hướng nếu người dùng vô tình vào nhầm URL /Admin/TaiKhoan/* hoặc /DuocSi/TaiKhoan/*
app.MapGet("/Admin/TaiKhoan/{action}", (string action) => Results.Redirect($"/TaiKhoan/{action}"));
app.MapPost("/Admin/TaiKhoan/{action}", (string action) => Results.Redirect($"/TaiKhoan/{action}"));
app.MapGet("/DuocSi/TaiKhoan/{action}", (string action) => Results.Redirect($"/TaiKhoan/{action}"));
app.MapPost("/DuocSi/TaiKhoan/{action}", (string action) => Results.Redirect($"/TaiKhoan/{action}"));

// Chuyển hướng an toàn nếu Admin vào nhầm /Admin/BanHang sang /DuocSi/BanHang
app.MapGet("/Admin/BanHang", (string? search) =>
    Results.Redirect(string.IsNullOrEmpty(search) ? "/DuocSi/BanHang" : $"/DuocSi/BanHang?search={Uri.EscapeDataString(search)}"));
app.MapGet("/Admin/BanHang/{action}", (string action, string? search) =>
    Results.Redirect(string.IsNullOrEmpty(search) ? $"/DuocSi/BanHang/{action}" : $"/DuocSi/BanHang/{action}?search={Uri.EscapeDataString(search)}"));
app.MapPost("/Admin/BanHang/{action}", (string action) =>
    Results.Redirect($"/DuocSi/BanHang/{action}"));

app.MapAreaControllerRoute(
    name: "admin_area",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "3" });

app.MapAreaControllerRoute(
    name: "duocsi_area",
    areaName: "DuocSi",
    pattern: "DuocSi/{controller=BanHang}/{action=Index}/{id?}")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "2,3" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
