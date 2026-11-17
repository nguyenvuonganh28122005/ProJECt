using System.ComponentModel.DataAnnotations;

namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class AccommodationIndexViewModel
{
    public string? Search { get; init; }
    public string? Type { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<string> Types { get; init; } = [];
    public IReadOnlyList<AccommodationRowViewModel> Items { get; init; } = [];
}

public sealed class AccommodationRowViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public string Address { get; init; } = "";
    public bool IsActive { get; init; }
    public string? CoverImage { get; init; }
}

public sealed class AccommodationEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = "";

    [Required, StringLength(100)]
    public string Type { get; set; } = "";

    [Required, StringLength(300)]
    public string Address { get; set; } = "";

    [StringLength(4000)]
    public string? Description { get; set; }
}

public sealed class AccommodationImagesViewModel
{
    public int AccommodationId { get; init; }
    public string AccommodationName { get; init; } = "";
    public IReadOnlyList<AccommodationImageViewModel> Images { get; init; } = [];
}

public sealed class AccommodationImageViewModel
{
    public int Id { get; init; }
    public string Url { get; init; } = "";
    public bool IsCover { get; init; }
}

public sealed class AddAccommodationImageViewModel
{
    public int AccommodationId { get; set; }

    [Required, Url, StringLength(500)]
    public string Url { get; set; } = "";
}
