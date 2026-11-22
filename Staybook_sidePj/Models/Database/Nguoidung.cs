using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Nguoidung
{
    public int MaNguoiDung { get; set; }

    public string HoTen { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string NormalizedUserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    public string? SoDt { get; set; }

    public string MatKhauHash { get; set; } = null!;

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    public string VaiTro { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public virtual ICollection<Danhgia> Danhgia { get; set; } = new List<Danhgia>();

    public virtual ICollection<Datphong> Datphong { get; set; } = new List<Datphong>();
}
