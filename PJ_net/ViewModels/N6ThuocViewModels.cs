using System.ComponentModel.DataAnnotations;
using PJ_net.Models;

namespace PJ_net.ViewModels
{
    public class ThuocListViewModel
    {
        public List<ThuocItemDto> Thuocs { get; set; } = new();
        public List<N6NhomThuoc> NhomThuocs { get; set; } = new();
        public string? Search { get; set; }
        public int? MaNhom { get; set; }
        public decimal? MinGia { get; set; }
        public decimal? MaxGia { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalItems { get; set; } = 0;
        public int PageSize { get; set; } = 12;
    }

    public class ThuocItemDto
    {
        public int MaThuoc { get; set; }
        public string TenThuoc { get; set; } = string.Empty;
        public string? HoatChat { get; set; }
        public string? DangBaoChe { get; set; }
        public string? DonViTinh { get; set; }
        public decimal GiaBan { get; set; }
        public bool KeDon { get; set; }
        public string? AnhThuoc { get; set; }
        public string? TenNhom { get; set; }
        public int TongTonKhaDung { get; set; }
    }

    public class ThuocDetailViewModel
    {
        public N6Thuoc Thuoc { get; set; } = null!;
        public int TongTonKhaDung { get; set; }
        public List<N6Thuoc> ThuocCungNhom { get; set; } = new();
    }
}
