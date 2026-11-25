using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Danhgia
{
    public int MaDanhGia { get; set; }

    public int MaNguoiDung { get; set; }

    public int MaNoiLuuTru { get; set; }

    public int DiemDanhGia { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual Nguoidung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Noiluutru MaNoiLuuTruNavigation { get; set; } = null!;
}
