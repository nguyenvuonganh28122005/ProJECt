using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class N6NhaCungCap
{
    public int MaNcc { get; set; }

    public string TenNcc { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<N6LoThuoc> LoThuocs { get; set; } = new List<N6LoThuoc>();
}
