using System;
using System.Collections.Generic;

namespace Staybook_sidePj.Models.Database;

public partial class Datphong
{
    public int MaDatPhong { get; set; }

    public int MaNguoiDung { get; set; }

    public int? MaNhanVienTao { get; set; }

    public int MaNoiLuuTru { get; set; }

    public DateTime NgayNhanPhong { get; set; }

    public DateTime NgayTraPhong { get; set; }

    public int SoLuongKhach { get; set; }

    public decimal TongTien { get; set; }

    public string TrangThaiDatPhong { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public virtual ICollection<Chitietdatphong> Chitietdatphong { get; set; } = new List<Chitietdatphong>();

    public virtual Nguoidung MaNguoiDungNavigation { get; set; } = null!;

    public virtual Noiluutru MaNoiLuuTruNavigation { get; set; } = null!;

    public virtual ICollection<Thanhtoan> Thanhtoan { get; set; } = new List<Thanhtoan>();
}
