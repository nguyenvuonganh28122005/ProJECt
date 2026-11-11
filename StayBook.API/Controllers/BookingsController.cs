using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayBook.API.DTOs;
using StayBook.API.Services;

namespace StayBook.API.Controllers;

[ApiController]
[Authorize]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingsController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult<DatPhongResponse>> Create(
        CreateDatPhongRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var result = await _bookingService.CreateAsync(userId, request, cancellationToken);
        if (result.Error is BookingError.InvalidDates)
        {
            return BadRequest(new { message = "Ngày nhận/trả phòng hoặc danh sách phòng không hợp lệ." });
        }

        if (result.Error is BookingError.RoomsNotFound)
        {
            return NotFound(new { message = "Không tìm thấy một hoặc nhiều phòng." });
        }

        if (result.Error is BookingError.RoomsUnavailable)
        {
            return Conflict(new { message = "Một hoặc nhiều phòng không còn khả dụng trong khoảng thời gian này." });
        }

        if (result.Error is BookingError.CapacityExceeded)
        {
            return BadRequest(new { message = "Tổng sức chứa của các phòng không đủ cho số khách." });
        }

        return StatusCode(StatusCodes.Status201Created, result.Booking);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<DatPhongResponse>>> GetMine(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _bookingService.GetForUserAsync(userId, cancellationToken));
    }
}
