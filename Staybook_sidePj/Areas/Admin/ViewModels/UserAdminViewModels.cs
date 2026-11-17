namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class UserIndexViewModel
{
    public string? Search { get; init; }
    public string? Role { get; init; }
    public bool? Active { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<UserRowViewModel> Items { get; init; } = [];
}

public sealed class UserRowViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string? Phone { get; init; }
    public string Role { get; init; } = "";
    public bool Active { get; init; }
}
