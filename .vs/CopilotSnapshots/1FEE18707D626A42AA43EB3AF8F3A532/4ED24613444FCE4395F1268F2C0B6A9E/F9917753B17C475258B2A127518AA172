using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Noiluutru
{
    public int MaNoiLuuTru { get; set; }

    public string TenNoiLuuTru { get; set; } = null!;

    public string LoaiHinh { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string? MoTa { get; set; }

    public bool? TrangThaiHoatDong { get; set; }

    public virtual ICollection<Danhgia> Danhgia { get; set; } = new List<Danhgia>();

    public virtual ICollection<Datphong> Datphong { get; set; } = new List<Datphong>();

    public virtual ICollection<Hinhanhnoiluutru> Hinhanhnoiluutru { get; set; } = new List<Hinhanhnoiluutru>();

    public virtual ICollection<Loaiphong> Loaiphong { get; set; } = new List<Loaiphong>();
}
