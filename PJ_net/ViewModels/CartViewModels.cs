using System.ComponentModel.DataAnnotations;

namespace PJ_net.ViewModels
{
    public class CartItem
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public string? DonViTinh { get; set; }
        public string? AnhThuoc { get; set; }
        public bool KeDon { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => GiaBan * SoLuong;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal TongTien => Items.Sum(i => i.ThanhTien);
        public int TongSoLuong => Items.Sum(i => i.SoLuong);
        public bool CoThuocKeDon => Items.Any(i => i.KeDon);
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận.")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ.")]
        [RegularExpression(@"^[0-9+()\-\s]{8,15}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận thuốc.")]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string DiaChi { get; set; } = string.Empty;

        [Display(Name = "Ghi chú thêm")]
        public string? GhiChuKhachHang { get; set; }

        // BR02: Thuốc kê đơn
        [Display(Name = "Hình thức cung cấp đơn thuốc")]
        public string? LoaiDonTuVan { get; set; } // "GuiAnh" hoặc "GuiMaDon"

        [Display(Name = "Mã đơn thuốc điện tử quốc gia")]
        public string? MaDonThuocDienTu { get; set; }

        public CartViewModel Cart { get; set; } = new();
    }
}
