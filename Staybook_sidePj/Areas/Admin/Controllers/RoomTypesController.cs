using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Areas.Admin.ViewModels;
using Staybook_sidePj.Data;
using Staybook_sidePj.Models.Database;

namespace Staybook_sidePj.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class RoomTypesController(StaybookDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, int? accommodationId, int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;
        page = Math.Max(1, page);
        var query = dbContext.Loaiphong.AsNoTracking()
            .Include(item => item.MaNoiLuuTruNavigation)
            .Include(item => item.MaTienNghi)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item => item.TenLoaiPhong.Contains(term));
        }
        if (accommodationId.HasValue)
        {
            query = query.Where(item => item.MaNoiLuuTru == accommodationId.Value);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(item => item.TenLoaiPhong)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new RoomTypeRowViewModel
            {
                Id = item.MaLoaiPhong,
                Name = item.TenLoaiPhong,
                AccommodationName = item.MaNoiLuuTruNavigation.TenNoiLuuTru,
                Capacity = item.SucChua,
                BaseRate = item.GiaCoBan,
                AmenityCount = item.MaTienNghi.Count
            })
            .ToListAsync(cancellationToken);

        return View(new RoomTypeIndexViewModel
        {
            Search = search,
            AccommodationId = accommodationId,
            Page = page,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            Accommodations = await GetAccommodationOptionsAsync(cancellationToken),
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new RoomTypeEditViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View("Edit", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomTypeEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View("Edit", model);
        }

        var amenities = await GetSelectedAmenitiesAsync(model.SelectedAmenityIds, cancellationToken);
        var item = new Loaiphong
        {
            MaNoiLuuTru = model.AccommodationId,
            TenLoaiPhong = model.Name.Trim(),
            MoTa = model.Description?.Trim(),
            SucChua = model.Capacity,
            GiaCoBan = model.BaseRate,
            MaTienNghi = amenities
        };
        dbContext.Loaiphong.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var item = await dbContext.Loaiphong.AsNoTracking()
            .Include(x => x.MaTienNghi)
            .SingleOrDefaultAsync(x => x.MaLoaiPhong == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        var model = new RoomTypeEditViewModel
        {
            Id = item.MaLoaiPhong,
            AccommodationId = item.MaNoiLuuTru,
            Name = item.TenLoaiPhong,
            Description = item.MoTa,
            Capacity = item.SucChua,
            BaseRate = item.GiaCoBan,
            SelectedAmenityIds = item.MaTienNghi.Select(x => x.MaTienNghi).ToList()
        };
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoomTypeEditViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || model.Id is null)
        {
            await PopulateOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var item = await dbContext.Loaiphong.Include(x => x.MaTienNghi)
            .SingleOrDefaultAsync(x => x.MaLoaiPhong == model.Id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        item.MaNoiLuuTru = model.AccommodationId;
        item.TenLoaiPhong = model.Name.Trim();
        item.MoTa = model.Description?.Trim();
        item.SucChua = model.Capacity;
        item.GiaCoBan = model.BaseRate;
        item.MaTienNghi = await GetSelectedAmenitiesAsync(model.SelectedAmenityIds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var item = await dbContext.Loaiphong.Include(x => x.Phong)
            .SingleOrDefaultAsync(x => x.MaLoaiPhong == id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }
        if (item.Phong.Count > 0)
        {
            TempData["Error"] = "Không thể xóa loại phòng đang có phòng cụ thể.";
            return RedirectToAction(nameof(Index));
        }

        dbContext.Loaiphong.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(RoomTypeEditViewModel model, CancellationToken cancellationToken)
    {
        model.Accommodations = await GetAccommodationOptionsAsync(cancellationToken);
        var selectedIds = model.SelectedAmenityIds.ToHashSet();
        model.Amenities = await dbContext.Tiennghi.AsNoTracking()
            .OrderBy(x => x.TenTienNghi)
            .Select(x => new AmenityOptionViewModel
            {
                Id = x.MaTienNghi,
                Name = x.TenTienNghi,
                Selected = selectedIds.Contains(x.MaTienNghi)
            })
            .ToListAsync(cancellationToken);
    }

    private Task<List<AccommodationOptionViewModel>> GetAccommodationOptionsAsync(CancellationToken cancellationToken) =>
        dbContext.Noiluutru.AsNoTracking()
            .OrderBy(x => x.TenNoiLuuTru)
            .Select(x => new AccommodationOptionViewModel { Id = x.MaNoiLuuTru, Name = x.TenNoiLuuTru })
            .ToListAsync(cancellationToken);

    private Task<List<Tiennghi>> GetSelectedAmenitiesAsync(IEnumerable<int> selectedIds, CancellationToken cancellationToken)
    {
        var ids = selectedIds.Distinct().ToArray();
        return dbContext.Tiennghi.Where(x => ids.Contains(x.MaTienNghi)).ToListAsync(cancellationToken);
    }
}
