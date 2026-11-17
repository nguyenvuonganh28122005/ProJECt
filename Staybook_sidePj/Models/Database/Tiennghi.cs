using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Tiennghi
{
    public int MaTienNghi { get; set; }

    public string TenTienNghi { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Loaiphong> MaLoaiPhong { get; set; } = new List<Loaiphong>();
}
