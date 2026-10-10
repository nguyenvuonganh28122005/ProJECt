using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class NhaCungCap
{
    public int MaNcc { get; set; }

    public string TenNcc { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? DiaChi { get; set; }

    public virtual ICollection<LoThuoc> LoThuocs { get; set; } = new List<LoThuoc>();
}
