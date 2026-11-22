using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Data;
using Staybook_sidePj.Models.Database;
using Staybook_sidePj.ViewModels;

namespace Staybook_sidePj.Controllers;

public sealed class StaysController(StaybookDbContext dbContext) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string? type,
        decimal? minimumPrice,
        decimal? maximumPrice,
        DateTime? checkIn,
        DateTime? checkOut,
        int guestCount = 1,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Noiluutru.AsNoTracking()
            .Where(item => item.TrangThaiHoatDong == true);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(item => item.TenNoiLuuTru.Contains(term) || item.DiaChi.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(item => item.LoaiHinh == type);
        }

        if (minimumPrice.HasValue || maximumPrice.HasValue)
        {
            query = query.Where(item => item.Loaiphong.Any(roomType =>
                (!minimumPrice.HasValue || roomType.GiaCoBan >= minimumPrice.Value)
                && (!maximumPrice.HasValue || roomType.GiaCoBan <= maximumPrice.Value)));
        }

        if (guestCount > 0)
        {
            query = query.Where(item => item.Loaiphong.Any(roomType => roomType.SucChua >= guestCount));
        }

        if (checkIn.HasValue && checkOut.HasValue && checkOut.Value.Date > checkIn.Value.Date)
        {
            query = query.Where(item => item.Loaiphong.Any(roomType => roomType.Phong.Any(room =>
                room.TrangThaiPhong == "Trống"
                && !room.Chitietdatphong.Any(detail =>
                    detail.MaDatPhongNavigation.TrangThaiDatPhong != "Đã hủy"
                    && detail.MaDatPhongNavigation.NgayNhanPhong < checkOut.Value.Date
                    && detail.MaDatPhongNavigation.NgayTraPhong > checkIn.Value.Date))));
        }

        var items = await query
            .OrderBy(item => item.TenNoiLuuTru)
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

        var types = await dbContext.Noiluutru.AsNoTracking()
            .Where(item => item.TrangThaiHoatDong == true)
            .Select(item => item.LoaiHinh)
            .Distinct()
            .OrderBy(item => item)
            .ToListAsync(cancellationToken);

        return View(new AccommodationSearchViewModel
        {
            Search = search,
            Type = type,
            MinimumPrice = minimumPrice,
            MaximumPrice = maximumPrice,
            CheckIn = checkIn?.Date,
            CheckOut = checkOut?.Date,
            GuestCount = Math.Clamp(guestCount, 1, 100),
            Types = types,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var item = await dbContext.Noiluutru.AsNoTracking()
            .Where(accommodation => accommodation.MaNoiLuuTru == id && accommodation.TrangThaiHoatDong == true)
            .Select(accommodation => new AccommodationDetailsViewModel
            {
                Id = accommodation.MaNoiLuuTru,
                Name = accommodation.TenNoiLuuTru,
                Type = accommodation.LoaiHinh,
                Address = accommodation.DiaChi,
                Description = accommodation.MoTa,
                Images = accommodation.Hinhanhnoiluutru
                    .OrderByDescending(image => image.LaAnhDaiDien)
                    .Select(image => image.DuongDanAnh)
                    .ToList(),
                RoomTypes = accommodation.Loaiphong.Select(roomType => new RoomTypeCardViewModel
                {
                    Id = roomType.MaLoaiPhong,
                    Name = roomType.TenLoaiPhong,
                    Description = roomType.MoTa,
                    Capacity = roomType.SucChua,
                    NightlyRate = roomType.GiaCoBan,
                    Amenities = roomType.MaTienNghi.Select(amenity => amenity.TenTienNghi).ToList()
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : View(item);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Book(int id, DateTime? checkIn, DateTime? checkOut, int guestCount = 1, CancellationToken cancellationToken = default)
    {
        var accommodation = await dbContext.Noiluutru.AsNoTracking()
            .SingleOrDefaultAsync(item => item.MaNoiLuuTru == id && item.TrangThaiHoatDong == true, cancellationToken);
        if (accommodation is null)
        {
            return NotFound();
        }

        var model = new CustomerBookingCreateViewModel
        {
            AccommodationId = id,
            AccommodationName = accommodation.TenNoiLuuTru,
            CheckIn = checkIn?.Date ?? DateTime.Today,
            CheckOut = checkOut?.Date ?? DateTime.Today.AddDays(1),
            GuestCount = Math.Clamp(guestCount, 1, 100)
        };
        await PopulateAvailableRoomsAsync(model, cancellationToken);
        return View(model);
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(CustomerBookingCreateViewModel model, CancellationToken cancellationToken)
    {
        var customerId = GetCustomerId();
        var accommodation = await dbContext.Noiluutru.AsNoTracking()
            .SingleOrDefaultAsync(item => item.MaNoiLuuTru == model.AccommodationId && item.TrangThaiHoatDong == true, cancellationToken);

        if (customerId is null)
        {
            return Challenge();
        }

        if (accommodation is null)
        {
            return NotFound();
        }

        model.AccommodationName = accommodation.TenNoiLuuTru;
        if (model.CheckIn.Date < DateTime.Today || model.CheckOut.Date <= model.CheckIn.Date)
        {
            ModelState.AddModelError(string.Empty, "Ngày nhận/trả phòng không hợp lệ.");
        }

        if (model.SelectedRoomIds.Count == 0)
        {
            ModelState.AddModelError(nameof(model.SelectedRoomIds), "Vui lòng chọn ít nhất một phòng.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateAvailableRoomsAsync(model, cancellationToken);
            return View(model);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var activeCustomer = await dbContext.Nguoidung.AnyAsync(
            user => user.MaNguoiDung == customerId.Value && user.TrangThaiHoatDong == true,
            cancellationToken);
        if (!activeCustomer)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Challenge();
        }

        var availableRooms = await FindAvailableRoomsAsync(
            model.AccommodationId,
            model.CheckIn.Date,
            model.CheckOut.Date,
            model.GuestCount,
            cancellationToken);
        var selectedIds = model.SelectedRoomIds.Distinct().ToArray();
        var selectedRooms = availableRooms.Where(room => selectedIds.Contains(room.Id)).ToList();

        if (selectedRooms.Count != selectedIds.Length)
        {
            await transaction.RollbackAsync(cancellationToken);
            ModelState.AddModelError(nameof(model.SelectedRoomIds), "Một hoặc nhiều phòng không còn trống. Vui lòng chọn lại.");
            model.AvailableRooms = availableRooms;
            return View(model);
        }

        var nights = (model.CheckOut.Date - model.CheckIn.Date).Days;
        var booking = new Datphong
        {
            MaNguoiDung = customerId.Value,
            MaNoiLuuTru = model.AccommodationId,
            NgayNhanPhong = model.CheckIn.Date,
            NgayTraPhong = model.CheckOut.Date,
            SoLuongKhach = model.GuestCount,
            TongTien = selectedRooms.Sum(room => room.NightlyRate) * nights,
            TrangThaiDatPhong = "Chờ xác nhận",
            Chitietdatphong = selectedRooms.Select(room => new Chitietdatphong
            {
                MaPhong = room.Id,
                GiaMoiDem = room.NightlyRate,
                TrangThaiNhanPhong = "Chưa nhận",
                TrangThaiTraPhong = "Chưa trả"
            }).ToList()
        };

        dbContext.Datphong.Add(booking);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return RedirectToAction("MyBookings", "Account");
    }

    private int? GetCustomerId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    private async Task PopulateAvailableRoomsAsync(CustomerBookingCreateViewModel model, CancellationToken cancellationToken)
    {
        if (model.CheckIn.Date < DateTime.Today
            || model.CheckOut.Date <= model.CheckIn.Date
            || model.GuestCount < 1)
        {
            model.AvailableRooms = [];
            return;
        }

        model.AvailableRooms = await FindAvailableRoomsAsync(
            model.AccommodationId,
            model.CheckIn.Date,
            model.CheckOut.Date,
            model.GuestCount,
            cancellationToken);
    }

    private Task<List<CustomerAvailableRoomViewModel>> FindAvailableRoomsAsync(
        int accommodationId,
        DateTime checkIn,
        DateTime checkOut,
        int guests,
        CancellationToken cancellationToken) =>
        dbContext.Phong.AsNoTracking()
            .Where(room => room.TrangThaiPhong == "Trống"
                && room.MaLoaiPhongNavigation.MaNoiLuuTru == accommodationId
                && room.MaLoaiPhongNavigation.SucChua >= guests
                && !room.Chitietdatphong.Any(detail =>
                    detail.MaDatPhongNavigation.TrangThaiDatPhong != "Đã hủy"
                    && detail.NgayCheckOutThucTe == null
                    && detail.MaDatPhongNavigation.NgayNhanPhong < checkOut
                    && detail.MaDatPhongNavigation.NgayTraPhong > checkIn))
            .OrderBy(room => room.SoPhong)
            .Select(room => new CustomerAvailableRoomViewModel
            {
                Id = room.MaPhong,
                RoomNumber = room.SoPhong,
                RoomType = room.MaLoaiPhongNavigation.TenLoaiPhong,
                Capacity = room.MaLoaiPhongNavigation.SucChua,
                NightlyRate = room.MaLoaiPhongNavigation.GiaCoBan
            })
            .ToListAsync(cancellationToken);
}
