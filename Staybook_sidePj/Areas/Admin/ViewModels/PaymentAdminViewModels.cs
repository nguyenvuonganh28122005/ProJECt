namespace Staybook_sidePj.Areas.Admin.ViewModels;

public sealed class PaymentIndexViewModel
{
    public string? Status { get; init; }
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyList<string> Statuses { get; init; } = [];
    public IReadOnlyList<PaymentRowViewModel> Items { get; init; } = [];
}

public sealed class PaymentRowViewModel
{
    public int Id { get; init; }
    public int BookingId { get; init; }
    public string CustomerName { get; init; } = "";
    public string Method { get; init; } = "";
    public decimal Amount { get; init; }
    public string? TransactionCode { get; init; }
    public DateTime? PaidAt { get; init; }
    public string Status { get; init; } = "";
}
