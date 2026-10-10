CREATE DATABASE PJ_NET
-- 1. Bảng Tài khoản (Khách hàng, Dược sĩ, Admin)
CREATE TABLE TaiKhoan (
    MaTK INT PRIMARY KEY IDENTITY(1,1),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15),
    VaiTro INT CHECK (VaiTro IN (1, 2, 3)) DEFAULT 1
);

-- 2. Bảng Nhóm thuốc
CREATE TABLE NhomThuoc (
    MaNhom INT PRIMARY KEY IDENTITY(1,1),
    TenNhom NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);

-- 3. Bảng Thuốc
CREATE TABLE Thuoc (
    MaThuoc INT PRIMARY KEY IDENTITY(1,1),
    TenThuoc NVARCHAR(150) NOT NULL,
    HoatChat NVARCHAR(150),
    DangBaoChe NVARCHAR(50),
    DonViTinh NVARCHAR(20),
    GiaBan DECIMAL(18,2) NOT NULL,
    KeDon BIT DEFAULT 0, -- 1: Thuốc kê đơn, 0: Không kê đơn
    AnhThuoc VARCHAR(255),
    HuongDanSuDung NVARCHAR(MAX),
    MaNhom INT FOREIGN KEY REFERENCES NhomThuoc(MaNhom)
);

-- 4. Bảng Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC INT PRIMARY KEY IDENTITY(1,1),
    TenNCC NVARCHAR(150) NOT NULL,
    SoDienThoai VARCHAR(15),
    DiaChi NVARCHAR(255)
);

-- 5. Bảng Lô thuốc (Quản lý FEFO & Tồn kho)
CREATE TABLE LoThuoc (
    MaLo INT PRIMARY KEY IDENTITY(1,1),
    MaThuoc INT FOREIGN KEY REFERENCES Thuoc(MaThuoc),
    MaNCC INT FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    SoLo VARCHAR(50) NOT NULL,
    NgaySX DATE,
    HanSuDung DATE NOT NULL,
    SoLuongTon INT NOT NULL DEFAULT 0,
    GiaNhap DECIMAL(18,2) NOT NULL
);

-- 6. Bảng Hóa đơn & Đơn tư vấn (Hỗ trợ Ảnh đơn + Mã đơn điện tử)
CREATE TABLE HoaDon (
    MaHD INT PRIMARY KEY IDENTITY(1,1),
    MaKH INT NULL FOREIGN KEY REFERENCES TaiKhoan(MaTK), -- NULL nếu bán lẻ tại quầy
    NgayLap DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18,2) DEFAULT 0,
    HinhThuc NVARCHAR(20) CHECK (HinhThuc IN (N'TaiQuay', N'TrucTuyen')),
    
    -- Hỗ trợ 2 kiểu tư vấn đơn thuốc
    LoaiDonTuVan NVARCHAR(20) NULL CHECK (LoaiDonTuVan IN (N'GuiAnh', N'GuiMaDon')),
    AnhDonThuoc VARCHAR(255) NULL,        -- Dùng khi LoaiDonTuVan = 'GuiAnh'
    MaDonThuocDienTu VARCHAR(50) NULL,   -- Dùng khi LoaiDonTuVan = 'GuiMaDon'
    GhiChuKhachHang NVARCHAR(255) NULL,
    
    TrangThai NVARCHAR(30) DEFAULT N'ChoXacNhan' 
    -- 'ChoXacNhan', 'DaXacNhan', 'DangGiao', 'HoanTat', 'DaHuy'
);

-- 7. Chi tiết hóa đơn
CREATE TABLE ChiTietHoaDon (
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaLo INT FOREIGN KEY REFERENCES LoThuoc(MaLo),
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHD, MaLo)
);