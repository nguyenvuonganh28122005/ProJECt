using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Loaiphong
{
    public int MaLoaiPhong { get; set; }

    public int MaNoiLuuTru { get; set; }

    public string TenLoaiPhong { get; set; } = null!;

    public string? MoTa { get; set; }

    public int SucChua { get; set; }

    public decimal GiaCoBan { get; set; }

    public virtual Noiluutru MaNoiLuuTruNavigation { get; set; } = null!;

    public virtual ICollection<Phong> Phong { get; set; } = new List<Phong>();

    public virtual ICollection<Tiennghi> MaTienNghi { get; set; } = new List<Tiennghi>();
}
