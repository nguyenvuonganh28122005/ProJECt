using System.ComponentModel.DataAnnotations;

namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class AmenityIndexViewModel
{
    public string? Search { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<AmenityRowViewModel> Items { get; init; } = [];
}

public sealed class AmenityRowViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public int RoomTypeCount { get; init; }
}

public sealed class AmenityEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }
}
