
namespace StayBook.API.Models
{
    // 1. Người dùng
    public class NguoiDung
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoDT { get; set; } = "";
        public string MatKhauHash { get; set; } = "";
        public bool TrangThaiHoatDong { get; set; } = true;
        public string VaiTro { get; set; } = "KhachHang";

        public ICollection<DatPhong> DatPhongs { get; set; } = new List<DatPhong>();
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }

    // 2. Nơi lưu trú
    public class NoiLuuTru
    {
        public int Id { get; set; }
        public string TenNoiLuuTru { get; set; } = "";
        public string LoaiHinh { get; set; } = "";
        public string DiaChi { get; set; } = "";
        public string MoTa { get; set; } = "";
        public bool TrangThaiHoatDong { get; set; } = true;

        public ICollection<HinhAnhNoiLuuTru> HinhAnhs { get; set; } = new List<HinhAnhNoiLuuTru>();
        public ICollection<LoaiPhong> LoaiPhongs { get; set; } = new List<LoaiPhong>();
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }

    // 3. Hình ảnh nơi lưu trú
    public class HinhAnhNoiLuuTru
    {
        public int Id { get; set; }
        public int MaNoiLuuTru { get; set; }
        public string DuongDanAnh { get; set; } = "";
        public bool LaAnhDaiDien { get; set; }

        public NoiLuuTru? NoiLuuTru { get; set; }
    }

    // 4. Loại phòng
    public class LoaiPhong
    {
        public int Id { get; set; }
        public int MaNoiLuuTru { get; set; }
        public string TenLoaiPhong { get; set; } = "";
        public string MoTa { get; set; } = "";
        public int SucChua { get; set; }
        public decimal GiaCoBan { get; set; }

        public NoiLuuTru? NoiLuuTru { get; set; }
        public ICollection<Phong> Phongs { get; set; } = new List<Phong>();
        public ICollection<LoaiPhongTienNghi> TienNghis { get; set; } = new List<LoaiPhongTienNghi>();
    }

    // 5. Phòng
    public class Phong
    {
        public int Id { get; set; }
        public int MaLoaiPhong { get; set; }
        public string SoPhong { get; set; } = "";
        public string TrangThaiPhong { get; set; } = "Trong";

        public LoaiPhong? LoaiPhong { get; set; }
        public ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = new List<ChiTietDatPhong>();
    }

    // 6. Tiện nghi
    public class TienNghi
    {
        public int Id { get; set; }
        public string TenTienNghi { get; set; } = "";
        public string MoTa { get; set; } = "";

        public ICollection<LoaiPhongTienNghi> LoaiPhongs { get; set; } = new List<LoaiPhongTienNghi>();
    }

    // 7. Liên kết loại phòng - tiện nghi
    public class LoaiPhongTienNghi
    {
        public int MaLoaiPhong { get; set; }
        public int MaTienNghi { get; set; }

        public LoaiPhong? LoaiPhong { get; set; }
        public TienNghi? TienNghi { get; set; }
    }

    // 8. Đặt phòng
    public class DatPhong
    {
        public int Id { get; set; }
        public int MaNguoiDung { get; set; }
        public DateTime NgayNhanPhong { get; set; }
        public DateTime NgayTraPhong { get; set; }
        public int SoLuongKhach { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiDatPhong { get; set; } = "ChoXacNhan";
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public NguoiDung? NguoiDung { get; set; }
        public ICollection<ChiTietDatPhong> ChiTietDatPhongs { get; set; } = new List<ChiTietDatPhong>();
        public ThanhToan? ThanhToan { get; set; }
    }

    // 9. Chi tiết đặt phòng
    public class ChiTietDatPhong
    {
        public int MaDatPhong { get; set; }
        public int MaPhong { get; set; }
        public decimal GiaMoiDem { get; set; }
        public string TrangThaiNhanPhong { get; set; } = "ChuaNhan";
        public string TrangThaiTraPhong { get; set; } = "ChuaTra";
        public DateTime? NgayCheckInThucTe { get; set; }
        public DateTime? NgayCheckOutThucTe { get; set; }

        public DatPhong? DatPhong { get; set; }
        public Phong? Phong { get; set; }
    }

    // 10. Thanh toán
    public class ThanhToan
    {
        public int Id { get; set; }
        public int MaDatPhong { get; set; }
        public string PhuongThucThanhToan { get; set; } = "";
        public decimal SoTien { get; set; }
        public string TrangThaiThanhToan { get; set; } = "ChuaThanhToan";
        public string? MaGiaoDich { get; set; }
        public DateTime? NgayThanhToan { get; set; }

        public DatPhong? DatPhong { get; set; }
    }

    // 11. Đánh giá
    public class DanhGia
    {
        public int Id { get; set; }
        public int MaNguoiDung { get; set; }
        public int MaNoiLuuTru { get; set; }
        public int DiemDanhGia { get; set; }
        public string NoiDung { get; set; } = "";
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public NguoiDung? NguoiDung { get; set; }
        public NoiLuuTru? NoiLuuTru { get; set; }
    }
}
