using System.ComponentModel.DataAnnotations;

namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class RoomTypeIndexViewModel
{
    public string? Search { get; init; }
    public int? AccommodationId { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<AccommodationOptionViewModel> Accommodations { get; init; } = [];
    public IReadOnlyList<RoomTypeRowViewModel> Items { get; init; } = [];
}

public sealed class RoomTypeRowViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string AccommodationName { get; init; } = "";
    public int Capacity { get; init; }
    public decimal BaseRate { get; init; }
    public int AmenityCount { get; init; }
}

public sealed class RoomTypeEditViewModel
{
    public int? Id { get; set; }
    [Range(1, int.MaxValue)]
    public int AccommodationId { get; set; }
    [Required, StringLength(150)]
    public string Name { get; set; } = "";
    [StringLength(4000)]
    public string? Description { get; set; }
    [Range(1, 100)]
    public int Capacity { get; set; } = 1;
    [Range(typeof(decimal), "0", "999999999")]
    public decimal BaseRate { get; set; }
    public List<int> SelectedAmenityIds { get; set; } = [];
    public IReadOnlyList<AccommodationOptionViewModel> Accommodations { get; set; } = [];
    public IReadOnlyList<AmenityOptionViewModel> Amenities { get; set; } = [];
}

public sealed class AccommodationOptionViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
}

public sealed class AmenityOptionViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public bool Selected { get; init; }
}
