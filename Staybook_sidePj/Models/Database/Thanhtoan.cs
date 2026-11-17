using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Thanhtoan
{
    public int MaThanhToan { get; set; }

    public int MaDatPhong { get; set; }

    public string PhuongThucThanhToan { get; set; } = null!;

    public decimal SoTien { get; set; }

    public string TrangThaiThanhToan { get; set; } = null!;

    public string? MaGiaoDich { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public virtual Datphong MaDatPhongNavigation { get; set; } = null!;
}
