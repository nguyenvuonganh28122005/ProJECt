using System.Data;
using Microsoft.EntityFrameworkCore;
using StayBook.API.Data;
using StayBook.API.DTOs;
using StayBook.API.Models;

namespace StayBook.API.Services;

public enum BookingError
{
    InvalidDates,
    RoomsNotFound,
    RoomsUnavailable,
    CapacityExceeded
}

public sealed record BookingResult(DatPhongResponse? Booking, BookingError? Error);

public sealed class BookingService
{
    private readonly StayBookDbContext _dbContext;

    public BookingService(StayBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookingResult> CreateAsync(
        int userId,
        CreateDatPhongRequest request,
        CancellationToken cancellationToken)
    {
        var checkIn = request.CheckIn;
        var checkOut = request.CheckOut;
        var roomIds = request.RoomIds.Distinct().ToArray();
        if (checkIn.Date < DateTime.UtcNow.Date || checkOut <= checkIn || roomIds.Length == 0)
        {
            return new BookingResult(null, BookingError.InvalidDates);
        }

        if (roomIds.Length != request.RoomIds.Count)
        {
            return new BookingResult(null, BookingError.RoomsUnavailable);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var rooms = await _dbContext.Phongs
            .Where(x => roomIds.Contains(x.Id))
            .Include(x => x.LoaiPhong)
            .Include(x => x.ChiTietDatPhongs)
                .ThenInclude(x => x.DatPhong)
            .ToListAsync(cancellationToken);

        if (rooms.Count != roomIds.Length)
        {
            return new BookingResult(null, BookingError.RoomsNotFound);
        }

        if (rooms.Any(room => room.TrangThaiPhong != "Trong" ||
            room.ChiTietDatPhongs.Any(detail => detail.DatPhong is not null &&
                detail.DatPhong.TrangThaiDatPhong != "DaHuy" &&
                detail.DatPhong.NgayNhanPhong < checkOut &&
                detail.DatPhong.NgayTraPhong > checkIn)))
        {
            return new BookingResult(null, BookingError.RoomsUnavailable);
        }

        if (rooms.Sum(room => room.LoaiPhong!.SucChua) < request.Guests)
        {
            return new BookingResult(null, BookingError.CapacityExceeded);
        }

        var nights = (checkOut.Date - checkIn.Date).Days;
        var booking = new DatPhong
        {
            MaNguoiDung = userId,
            NgayNhanPhong = checkIn,
            NgayTraPhong = checkOut,
            SoLuongKhach = request.Guests,
            TongTien = rooms.Sum(room => room.LoaiPhong!.GiaCoBan * nights),
            TrangThaiDatPhong = "ChoXacNhan",
            NgayTao = DateTime.UtcNow,
            ChiTietDatPhongs = rooms.Select(room => new ChiTietDatPhong
            {
                MaPhong = room.Id,
                GiaMoiDem = room.LoaiPhong!.GiaCoBan,
                Phong = room
            }).ToList()
        };

        _dbContext.DatPhongs.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new BookingResult(ToResponse(booking), null);
    }

    public async Task<IReadOnlyList<DatPhongResponse>> GetForUserAsync(int userId, CancellationToken cancellationToken)
    {
        var bookings = await _dbContext.DatPhongs
            .AsNoTracking()
            .Where(x => x.MaNguoiDung == userId)
            .Include(x => x.ChiTietDatPhongs)
                .ThenInclude(x => x.Phong)
                    .ThenInclude(x => x!.LoaiPhong)
                        .ThenInclude(x => x!.NoiLuuTru)
            .OrderByDescending(x => x.NgayTao)
            .ToListAsync(cancellationToken);

        return bookings.Select(ToResponse).ToList();
    }

    private static DatPhongResponse ToResponse(DatPhong booking) => new(
        booking.Id,
        booking.NgayNhanPhong,
        booking.NgayTraPhong,
        booking.SoLuongKhach,
        booking.TongTien,
        booking.TrangThaiDatPhong,
        booking.NgayTao,
        booking.ChiTietDatPhongs.Select(detail => new DatPhongRoomResponse(
            detail.MaPhong,
            detail.Phong?.SoPhong ?? string.Empty,
            detail.Phong?.LoaiPhong?.NoiLuuTru?.TenNoiLuuTru ?? string.Empty,
            detail.Phong?.LoaiPhong?.TenLoaiPhong ?? string.Empty,
            detail.GiaMoiDem)).ToList());
}
