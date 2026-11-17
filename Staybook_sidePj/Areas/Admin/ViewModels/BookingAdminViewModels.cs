using System.ComponentModel.DataAnnotations;

namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class BookingIndexViewModel
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<string> Statuses { get; init; } = [];
    public IReadOnlyList<BookingRowViewModel> Items { get; init; } = [];
}

public sealed class BookingRowViewModel
{
    public int Id { get; init; }
    public string GuestName { get; init; } = "";
    public string AccommodationName { get; init; } = "";
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public string Status { get; init; } = "";
    public string? PaymentStatus { get; init; }
    public decimal Total { get; init; }
}

public sealed class BookingDetailsViewModel
{
    public int Id { get; init; }
    public string GuestName { get; init; } = "";
    public string AccommodationName { get; init; } = "";
    public DateTime CheckIn { get; init; }
    public DateTime CheckOut { get; init; }
    public int GuestCount { get; init; }
    public string Status { get; init; } = "";
    public decimal Total { get; init; }
    public IReadOnlyList<BookingRoomViewModel> Rooms { get; init; } = [];
}

public sealed class BookingRoomViewModel
{
    public int RoomId { get; init; }
    public string RoomNumber { get; init; } = "";
    public string RoomType { get; init; } = "";
    public string CheckInStatus { get; init; } = "";
    public string CheckOutStatus { get; init; } = "";
    public DateTime? ActualCheckIn { get; init; }
    public DateTime? ActualCheckOut { get; init; }
}

public sealed class BookingCreateViewModel
{
    public int? CustomerId { get; set; }
    public int? AccommodationId { get; set; }
    [DataType(DataType.Date)]
    public DateTime CheckIn { get; set; } = DateTime.Today;
    [DataType(DataType.Date)]
    public DateTime CheckOut { get; set; } = DateTime.Today.AddDays(1);
    [Range(1, 100)]
    public int GuestCount { get; set; } = 1;
    public List<int> SelectedRoomIds { get; set; } = [];
    public IReadOnlyList<CustomerOptionViewModel> Customers { get; set; } = [];
    public IReadOnlyList<AccommodationOptionViewModel> Accommodations { get; set; } = [];
    public IReadOnlyList<AvailableRoomViewModel> AvailableRooms { get; set; } = [];
}

public sealed class CustomerOptionViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
}

public sealed class AvailableRoomViewModel
{
    public int Id { get; init; }
    public string RoomNumber { get; init; } = "";
    public string RoomType { get; init; } = "";
    public int Capacity { get; init; }
    public decimal NightlyRate { get; init; }
}
