namespace Staybook_sidePj.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyList<AccommodationCardViewModel> FeaturedAccommodations { get; init; } = [];
}
