using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Chitietdatphong
{
    public int MaDatPhong { get; set; }

    public int MaPhong { get; set; }

    public decimal GiaMoiDem { get; set; }

    public string TrangThaiNhanPhong { get; set; } = null!;

    public string TrangThaiTraPhong { get; set; } = null!;

    public DateTime? NgayCheckInThucTe { get; set; }

    public DateTime? NgayCheckOutThucTe { get; set; }

    public virtual Datphong MaDatPhongNavigation { get; set; } = null!;

    public virtual Phong MaPhongNavigation { get; set; } = null!;
}
