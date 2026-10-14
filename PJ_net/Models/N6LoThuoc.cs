using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class N6LoThuoc
{
    public int MaLo { get; set; }

    public int? MaThuoc { get; set; }

    public int? MaNcc { get; set; }

    public string SoLo { get; set; } = null!;

    public DateOnly? NgaySx { get; set; }

    public DateOnly HanSuDung { get; set; }

    public int SoLuongTon { get; set; }

    public decimal GiaNhap { get; set; }

    public virtual ICollection<N6ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<N6ChiTietHoaDon>();

    public virtual N6NhaCungCap? MaNccNavigation { get; set; }

    public virtual N6Thuoc? MaThuocNavigation { get; set; }
}
