namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class DashboardViewModel
{
    public decimal TotalRevenue { get; init; }
    public int BookingCount { get; init; }
    public int AccommodationCount { get; init; }
    public int UserCount { get; init; }
    public IReadOnlyList<RevenueMonthViewModel> MonthlyRevenue { get; init; } = [];
    public IReadOnlyList<RecentBookingViewModel> RecentBookings { get; init; } = [];
    public IReadOnlyList<RecentReviewViewModel> RecentReviews { get; init; } = [];
}

public sealed class RevenueMonthViewModel
{
    public string Label { get; init; } = "";
    public decimal Amount { get; init; }
    public double Percent { get; init; }
}

public sealed class RecentBookingViewModel
{
    public int Id { get; init; }
    public string GuestName { get; init; } = "";
    public string AccommodationName { get; init; } = "";
    public DateTime CheckIn { get; init; }
    public string Status { get; init; } = "";
    public decimal Total { get; init; }
}

public sealed class RecentReviewViewModel
{
    public int Id { get; init; }
    public string GuestName { get; init; } = "";
    public string AccommodationName { get; init; } = "";
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime? CreatedAt { get; init; }
}
