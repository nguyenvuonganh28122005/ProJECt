using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class N6ChiTietHoaDon
{
    public int MaHd { get; set; }

    public int MaLo { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public virtual N6HoaDon MaHdNavigation { get; set; } = null!;

    public virtual N6LoThuoc MaLoNavigation { get; set; } = null!;
}
