using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Areas.Admin.ViewModels;
using Staybook_sidePj.Data;

namespace Staybook_sidePj.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "QuanTriVien")]
public sealed class ReviewsController(StaybookDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? rating, string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 25;
        page = Math.Max(1, page);
        var query = dbContext.Danhgia.AsNoTracking()
            .Include(review => review.MaNguoiDungNavigation)
            .Include(review => review.MaNoiLuuTruNavigation)
            .AsQueryable();
        if (rating is >= 1 and <= 5)
        {
            query = query.Where(review => review.DiemDanhGia == rating.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(review => (review.NoiDung != null && review.NoiDung.Contains(term))
                || review.MaNguoiDungNavigation.HoTen.Contains(term)
                || review.MaNoiLuuTruNavigation.TenNoiLuuTru.Contains(term));
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(review => review.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(review => new ReviewRowViewModel
            {
                Id = review.MaDanhGia,
                CustomerName = review.MaNguoiDungNavigation.HoTen,
                AccommodationName = review.MaNoiLuuTruNavigation.TenNoiLuuTru,
                Rating = review.DiemDanhGia,
                Comment = review.NoiDung,
                CreatedAt = review.NgayTao
            })
            .ToListAsync(cancellationToken);
        return View(new ReviewIndexViewModel
        {
            Rating = rating,
            Search = search,
            Page = page,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            Items = items
        });
    }
}
