using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Hinhanhnoiluutru
{
    public int MaHinhAnh { get; set; }

    public int MaNoiLuuTru { get; set; }

    public string DuongDanAnh { get; set; } = null!;

    public bool LaAnhDaiDien { get; set; }

    public virtual Noiluutru MaNoiLuuTruNavigation { get; set; } = null!;
}
