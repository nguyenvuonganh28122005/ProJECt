namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class ReviewIndexViewModel
{
    public int? Rating { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<ReviewRowViewModel> Items { get; init; } = [];
}

public sealed class ReviewRowViewModel
{
    public int Id { get; init; }
    public string CustomerName { get; init; } = "";
    public string AccommodationName { get; init; } = "";
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime? CreatedAt { get; init; }
}
