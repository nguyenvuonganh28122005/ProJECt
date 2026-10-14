using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class N6HoaDon
{
    public int MaHd { get; set; }

    public int? MaKh { get; set; }

    public DateTime? NgayLap { get; set; }

    public decimal? TongTien { get; set; }

    public string? HinhThuc { get; set; }

    public string? LoaiDonTuVan { get; set; }

    public string? AnhDonThuoc { get; set; }

    public string? MaDonThuocDienTu { get; set; }

    public string? GhiChuKhachHang { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<N6ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<N6ChiTietHoaDon>();

    public virtual N6TaiKhoan? MaKhNavigation { get; set; }
}
