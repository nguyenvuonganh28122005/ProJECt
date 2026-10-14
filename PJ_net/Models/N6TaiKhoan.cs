using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class N6TaiKhoan
{
    public int MaTk { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public int? VaiTro { get; set; }

    public virtual ICollection<N6HoaDon> HoaDons { get; set; } = new List<N6HoaDon>();
}
