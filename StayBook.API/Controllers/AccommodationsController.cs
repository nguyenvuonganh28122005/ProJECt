using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StayBook.API.Data;
using StayBook.API.DTOs;
using StayBook.API.Models;

namespace StayBook.API.Controllers;

[ApiController]
[Route("api/accommodations")]
public sealed class AccommodationsController : ControllerBase
{
    private readonly StayBookDbContext _dbContext;

    public AccommodationsController(StayBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NoiLuuTruResponse>>> Search(
        [FromQuery] NoiLuuTruQuery query,
        CancellationToken cancellationToken)
    {
        if (!IsValidQuery(query))
        {
            return BadRequest(new { message = "Bộ lọc ngày, số khách, giá hoặc phân trang không hợp lệ." });
        }

        var accommodations = await BuildQuery(query)
            .OrderBy(x => x.TenNoiLuuTru)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Ok(accommodations.Select(x => ToResponse(x, query)).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NoiLuuTruResponse>> GetById(
        int id,
        [FromQuery] NoiLuuTruQuery query,
        CancellationToken cancellationToken)
    {
        if (!IsValidQuery(query))
        {
            return BadRequest(new { message = "Bộ lọc ngày, số khách hoặc giá không hợp lệ." });
        }

        var accommodation = await BuildQuery(query)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (accommodation is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(accommodation, query));
    }

    private IQueryable<NoiLuuTru> BuildQuery(NoiLuuTruQuery query)
    {
        var accommodations = _dbContext.NoiLuuTrus
            .AsNoTracking()
            .Where(x => x.TrangThaiHoatDong)
            .Include(x => x.HinhAnhs)
            .Include(x => x.LoaiPhongs)
                .ThenInclude(x => x.Phongs)
                    .ThenInclude(x => x.ChiTietDatPhongs)
                        .ThenInclude(x => x.DatPhong)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();
            accommodations = accommodations.Where(x =>
                x.TenNoiLuuTru.Contains(keyword) || x.MoTa.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim();
            accommodations = accommodations.Where(x => x.DiaChi.Contains(location));
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            var type = query.Type.Trim();
            accommodations = accommodations.Where(x => x.LoaiHinh == type);
        }

        var roomTypes = _dbContext.LoaiPhongs.AsQueryable();
        if (query.Guests.HasValue)
        {
            roomTypes = roomTypes.Where(x => x.SucChua >= query.Guests.Value);
        }

        if (query.MinPrice.HasValue)
        {
            roomTypes = roomTypes.Where(x => x.GiaCoBan >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            roomTypes = roomTypes.Where(x => x.GiaCoBan <= query.MaxPrice.Value);
        }

        var validRoomTypeIds = roomTypes.Select(x => x.Id);
        accommodations = accommodations.Where(x => x.LoaiPhongs.Any(roomType =>
            validRoomTypeIds.Contains(roomType.Id) && roomType.Phongs.Any(room => room.TrangThaiPhong == "Trong")));

        if (query.CheckIn.HasValue && query.CheckOut.HasValue)
        {
            var checkIn = query.CheckIn.Value;
            var checkOut = query.CheckOut.Value;
            accommodations = accommodations.Where(x => x.LoaiPhongs.Any(roomType =>
                validRoomTypeIds.Contains(roomType.Id) && roomType.Phongs.Any(room =>
                    room.TrangThaiPhong == "Trong" && !room.ChiTietDatPhongs.Any(detail =>
                        detail.DatPhong != null &&
                        detail.DatPhong.TrangThaiDatPhong != "DaHuy" &&
                        detail.DatPhong.NgayNhanPhong < checkOut &&
                        detail.DatPhong.NgayTraPhong > checkIn))));
        }

        return accommodations;
    }

    private static bool IsValidQuery(NoiLuuTruQuery query) =>
        (!query.CheckIn.HasValue && !query.CheckOut.HasValue ||
            query.CheckIn.HasValue && query.CheckOut.HasValue && query.CheckOut > query.CheckIn) &&
        (!query.Guests.HasValue || query.Guests > 0) &&
        (!query.MinPrice.HasValue || query.MinPrice >= 0) &&
        (!query.MaxPrice.HasValue || query.MaxPrice >= 0) &&
        (!query.MinPrice.HasValue || !query.MaxPrice.HasValue || query.MinPrice <= query.MaxPrice) &&
        query.Page >= 1 && query.PageSize is >= 1 and <= 100;

    private static bool IsVacant(Phong room) => room.TrangThaiPhong == "Trong";

    private static NoiLuuTruResponse ToResponse(NoiLuuTru accommodation, NoiLuuTruQuery query)
    {
        var roomTypes = accommodation.LoaiPhongs
            .Where(roomType => (!query.Guests.HasValue || roomType.SucChua >= query.Guests.Value) &&
                (!query.MinPrice.HasValue || roomType.GiaCoBan >= query.MinPrice.Value) &&
                (!query.MaxPrice.HasValue || roomType.GiaCoBan <= query.MaxPrice.Value))
            .Select(roomType => new LoaiPhongResponse(
                roomType.Id,
                roomType.TenLoaiPhong,
                roomType.MoTa,
                roomType.SucChua,
                roomType.GiaCoBan,
                roomType.Phongs
                    .Where(room => IsRoomAvailable(room, query))
                    .Select(room => new PhongResponse(room.Id, room.SoPhong, room.TrangThaiPhong))
                    .ToList()))
            .Where(roomType => roomType.Rooms.Count > 0)
            .ToList();

        return new NoiLuuTruResponse(
            accommodation.Id,
            accommodation.TenNoiLuuTru,
            accommodation.LoaiHinh,
            accommodation.DiaChi,
            accommodation.MoTa,
            accommodation.HinhAnhs.Select(image => image.DuongDanAnh).ToList(),
            roomTypes);
    }

    private static bool IsRoomAvailable(Phong room, NoiLuuTruQuery query)
    {
        if (!IsVacant(room))
        {
            return false;
        }

        if (!query.CheckIn.HasValue || !query.CheckOut.HasValue)
        {
            return true;
        }

        return !room.ChiTietDatPhongs.Any(detail =>
            detail.DatPhong is not null &&
            detail.DatPhong.TrangThaiDatPhong != "DaHuy" &&
            detail.DatPhong.NgayNhanPhong < query.CheckOut.Value &&
            detail.DatPhong.NgayTraPhong > query.CheckIn.Value);
    }
}
