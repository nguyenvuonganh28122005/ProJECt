using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class ChiTietHoaDon
{
    public int MaHd { get; set; }

    public int MaLo { get; set; }

    public int SoLuong { get; set; }

    public decimal DonGia { get; set; }

    public virtual HoaDon MaHdNavigation { get; set; } = null!;

    public virtual LoThuoc MaLoNavigation { get; set; } = null!;
}
