using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Phong
{
    public int MaPhong { get; set; }

    public int MaLoaiPhong { get; set; }

    public string SoPhong { get; set; } = null!;

    public string TrangThaiPhong { get; set; } = null!;

    public virtual ICollection<Chitietdatphong> Chitietdatphong { get; set; } = new List<Chitietdatphong>();

    public virtual Loaiphong MaLoaiPhongNavigation { get; set; } = null!;
}
