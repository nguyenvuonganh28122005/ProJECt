using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Data;
using Staybook_sidePj.Models;
using Staybook_sidePj.ViewModels;
using System.Diagnostics;

namespace Staybook_sidePj.Controllers
{
    public class HomeController(StaybookDbContext dbContext) : Controller
    {
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var featured = await dbContext.Noiluutru.AsNoTracking()
                .Where(item => item.TrangThaiHoatDong == true)
                .OrderBy(item => item.TenNoiLuuTru)
                .Take(3)
                .Select(item => new AccommodationCardViewModel
                {
                    Id = item.MaNoiLuuTru,
                    Name = item.TenNoiLuuTru,
                    Type = item.LoaiHinh,
                    Address = item.DiaChi,
                    Description = item.MoTa,
                    ImageUrl = item.Hinhanhnoiluutru
                        .OrderByDescending(image => image.LaAnhDaiDien)
                        .Select(image => image.DuongDanAnh)
                        .FirstOrDefault(),
                    StartingPrice = item.Loaiphong
                        .Select(roomType => (decimal?)roomType.GiaCoBan)
                        .Min()
                })
                .ToListAsync(cancellationToken);

            return View(new HomeViewModel { FeaturedAccommodations = featured });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
