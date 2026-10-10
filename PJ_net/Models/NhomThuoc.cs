using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class NhomThuoc
{
    public int MaNhom { get; set; }

    public string TenNhom { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<Thuoc> Thuocs { get; set; } = new List<Thuoc>();
}
