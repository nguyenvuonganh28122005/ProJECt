using System;
using System.Collections.Generic;

namespace PJ_net.Models;

public partial class LoThuoc
{
    public int MaLo { get; set; }

    public int? MaThuoc { get; set; }

    public int? MaNcc { get; set; }

    public string SoLo { get; set; } = null!;

    public DateOnly? NgaySx { get; set; }

    public DateOnly HanSuDung { get; set; }

    public int SoLuongTon { get; set; }

    public decimal GiaNhap { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual NhaCungCap? MaNccNavigation { get; set; }

    public virtual Thuoc? MaThuocNavigation { get; set; }
}
