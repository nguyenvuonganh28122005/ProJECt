using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Nguoidung
{
    public int MaNguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? SoDt { get; set; }

    public string MatKhauHash { get; set; } = null!;

    public bool TrangThaiHoatDong { get; set; }

    public string VaiTro { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public virtual ICollection<Danhgia> Danhgia { get; set; } = new List<Danhgia>();

    public virtual ICollection<Datphong> Datphong { get; set; } = new List<Datphong>();
}
