using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Areas.Admin.ViewModels;
using Staybook_sidePj.Data;

namespace Staybook_sidePj.Areas.Admin.Controllers;

[Area("Admin")]
public sealed class PaymentsController(StaybookDbContext dbContext) : Controller
{
    private static readonly string[] EditableStatuses = ["Chờ thanh toán", "Đã thanh toán", "Đã hoàn tiền", "Thất bại"];

    [HttpGet]
    public async Task<IActionResult> Index(string? status, int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 25;
        page = Math.Max(1, page);
        var query = dbContext.Thanhtoan.AsNoTracking()
            .Include(payment => payment.MaDatPhongNavigation)
                .ThenInclude(booking => booking.MaNguoiDungNavigation)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(payment => payment.TrangThaiThanhToan == status);
        }

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(payment => payment.NgayThanhToan)
            .ThenByDescending(payment => payment.MaThanhToan)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(payment => new PaymentRowViewModel
            {
                Id = payment.MaThanhToan,
                BookingId = payment.MaDatPhong,
                CustomerName = payment.MaDatPhongNavigation.MaNguoiDungNavigation.HoTen,
                Method = payment.PhuongThucThanhToan,
                Amount = payment.SoTien,
                TransactionCode = payment.MaGiaoDich,
                PaidAt = payment.NgayThanhToan,
                Status = payment.TrangThaiThanhToan
            })
            .ToListAsync(cancellationToken);
        var statuses = await dbContext.Thanhtoan.AsNoTracking()
            .Select(payment => payment.TrangThaiThanhToan).Distinct().OrderBy(x => x).ToListAsync(cancellationToken);
        return View(new PaymentIndexViewModel
        {
            Status = status,
            Page = page,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            Statuses = statuses,
            Items = items
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, CancellationToken cancellationToken)
    {
        if (!EditableStatuses.Contains(status))
        {
            return BadRequest("Trạng thái thanh toán không hợp lệ.");
        }

        var payment = await dbContext.Thanhtoan.FindAsync([id], cancellationToken);
        if (payment is null)
        {
            return NotFound();
        }

        payment.TrangThaiThanhToan = status;
        if (status == "Đã thanh toán" && payment.NgayThanhToan is null)
        {
            payment.NgayThanhToan = DateTime.Now;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
