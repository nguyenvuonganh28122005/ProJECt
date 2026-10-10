using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class Thuoc
{
    public int MaThuoc { get; set; }

    public string TenThuoc { get; set; } = null!;

    public string? HoatChat { get; set; }

    public string? DangBaoChe { get; set; }

    public string? DonViTinh { get; set; }

    public decimal GiaBan { get; set; }

    public bool? KeDon { get; set; }

    public string? AnhThuoc { get; set; }

    public string? HuongDanSuDung { get; set; }

    public int? MaNhom { get; set; }

    public virtual ICollection<LoThuoc> LoThuocs { get; set; } = new List<LoThuoc>();

    public virtual NhomThuoc? MaNhomNavigation { get; set; }
}
