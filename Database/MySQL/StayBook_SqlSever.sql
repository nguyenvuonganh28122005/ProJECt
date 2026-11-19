/*
    STAYBOOK - SQL Server database initialization script
    Version: revised for multi-room bookings

    IMPORTANT:
    - This script is intended for a NEW / EMPTY StayBook database.
    - It does not delete existing tables or data.
    - If your old StayBook database already contains tables (for example KhachHang),
      back it up and migrate it separately before running this initialization script.
*/

IF DB_ID(N'StayBook') IS NULL
BEGIN
    CREATE DATABASE StayBook;
END;
GO

USE StayBook;
GO

/* Stop rather than silently mixing this schema with the previous schema. */
IF OBJECT_ID(N'dbo.KhachHang', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.NguoiDung', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.DatPhong', N'U') IS NOT NULL
BEGIN
    THROW 50001,
        'Database StayBook already contains tables. Back up/migrate the existing database or use a new empty database before running this initialization script.',
        1;
END;
GO

/* 1. Users: customers, staff and administrators share one account table. */
CREATE TABLE dbo.NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_NguoiDung PRIMARY KEY,
    HoTen NVARCHAR(150) NOT NULL,
    Email NVARCHAR(255) NOT NULL
        CONSTRAINT UQ_NguoiDung_Email UNIQUE,
    SoDT VARCHAR(20) NULL,
    MatKhauHash NVARCHAR(500) NOT NULL,
    TrangThaiHoatDong BIT NOT NULL
        CONSTRAINT DF_NguoiDung_TrangThaiHoatDong DEFAULT (1),
    VaiTro NVARCHAR(30) NOT NULL,
    NgayTao DATETIME2 NOT NULL
        CONSTRAINT DF_NguoiDung_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT CK_NguoiDung_VaiTro
        CHECK (VaiTro IN (N'KhachHang', N'NhanVien', N'QuanTriVien'))
);
GO

/* 2. Accommodation. */
CREATE TABLE dbo.NoiLuuTru (
    MaNoiLuuTru INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_NoiLuuTru PRIMARY KEY,
    TenNoiLuuTru NVARCHAR(200) NOT NULL,
    LoaiHinh NVARCHAR(50) NOT NULL,
    DiaChi NVARCHAR(300) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    TrangThaiHoatDong BIT NOT NULL
        CONSTRAINT DF_NoiLuuTru_TrangThai DEFAULT (1)
);
GO

CREATE TABLE dbo.HinhAnhNoiLuuTru (
    MaHinhAnh INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_HinhAnhNoiLuuTru PRIMARY KEY,
    MaNoiLuuTru INT NOT NULL,
    DuongDanAnh NVARCHAR(500) NOT NULL,
    LaAnhDaiDien BIT NOT NULL
        CONSTRAINT DF_HinhAnh_LaAnhDaiDien DEFAULT (0),
    CONSTRAINT FK_HinhAnhNoiLuuTru_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru)
        REFERENCES dbo.NoiLuuTru(MaNoiLuuTru)
);
GO

/* 3. Room types and physical rooms. */
CREATE TABLE dbo.LoaiPhong (
    MaLoaiPhong INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_LoaiPhong PRIMARY KEY,
    MaNoiLuuTru INT NOT NULL,
    TenLoaiPhong NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    SucChua INT NOT NULL,
    GiaCoBan DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_LoaiPhong_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru)
        REFERENCES dbo.NoiLuuTru(MaNoiLuuTru),
    CONSTRAINT CK_LoaiPhong_SucChua CHECK (SucChua > 0),
    CONSTRAINT CK_LoaiPhong_GiaCoBan CHECK (GiaCoBan >= 0)
);
GO

CREATE TABLE dbo.Phong (
    MaPhong INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Phong PRIMARY KEY,
    MaLoaiPhong INT NOT NULL,
    SoPhong NVARCHAR(30) NOT NULL,
    TrangThaiPhong NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Phong_LoaiPhong
        FOREIGN KEY (MaLoaiPhong)
        REFERENCES dbo.LoaiPhong(MaLoaiPhong),
    CONSTRAINT UQ_Phong_LoaiPhong_SoPhong UNIQUE (MaLoaiPhong, SoPhong)
);
GO

/* 4. Amenities and room-type/amenity many-to-many relation. */
CREATE TABLE dbo.TienNghi (
    MaTienNghi INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_TienNghi PRIMARY KEY,
    TenTienNghi NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(MAX) NULL
);
GO

CREATE TABLE dbo.LoaiPhong_TienNghi (
    MaLoaiPhong INT NOT NULL,
    MaTienNghi INT NOT NULL,
    CONSTRAINT PK_LoaiPhong_TienNghi
        PRIMARY KEY (MaLoaiPhong, MaTienNghi),
    CONSTRAINT FK_LPTN_LoaiPhong
        FOREIGN KEY (MaLoaiPhong)
        REFERENCES dbo.LoaiPhong(MaLoaiPhong),
    CONSTRAINT FK_LPTN_TienNghi
        FOREIGN KEY (MaTienNghi)
        REFERENCES dbo.TienNghi(MaTienNghi)
);
GO

/* 5. Booking header.
      MaNguoiDung is the customer who owns the booking.
      MaNhanVienTao is nullable: NULL means the customer booked by themselves.
      If staff creates the booking, it references that staff member's NguoiDung row.
*/
CREATE TABLE dbo.DatPhong (
    MaDatPhong INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_DatPhong PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaNhanVienTao INT NULL,
    NgayNhanPhong DATE NOT NULL,
    NgayTraPhong DATE NOT NULL,
    SoLuongKhach INT NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThaiDatPhong NVARCHAR(50) NOT NULL,
    NgayTao DATETIME2 NOT NULL
        CONSTRAINT DF_DatPhong_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_DatPhong_NguoiDung
        FOREIGN KEY (MaNguoiDung)
        REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DatPhong_NhanVienTao
        FOREIGN KEY (MaNhanVienTao)
        REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT CK_DatPhong_Ngay CHECK (NgayTraPhong > NgayNhanPhong),
    CONSTRAINT CK_DatPhong_SoLuongKhach CHECK (SoLuongKhach > 0),
    CONSTRAINT CK_DatPhong_TongTien CHECK (TongTien >= 0)
);
GO

/* 6. Booking details.
      Composite PK allows one booking to contain multiple distinct rooms.
      The same room can appear in another booking; date-overlap validation
      must also be enforced by backend booking logic/transaction.
*/
CREATE TABLE dbo.ChiTietDatPhong (
    MaDatPhong INT NOT NULL,
    MaPhong INT NOT NULL,
    GiaMoiDem DECIMAL(18,2) NOT NULL,
    TrangThaiNhanPhong NVARCHAR(50) NULL,
    TrangThaiTraPhong NVARCHAR(50) NULL,
    NgayCheckInThucTe DATETIME2 NULL,
    NgayCheckOutThucTe DATETIME2 NULL,
    CONSTRAINT PK_ChiTietDatPhong
        PRIMARY KEY (MaDatPhong, MaPhong),
    CONSTRAINT FK_ChiTietDatPhong_DatPhong
        FOREIGN KEY (MaDatPhong)
        REFERENCES dbo.DatPhong(MaDatPhong),
    CONSTRAINT FK_ChiTietDatPhong_Phong
        FOREIGN KEY (MaPhong)
        REFERENCES dbo.Phong(MaPhong),
    CONSTRAINT CK_ChiTietDatPhong_GiaMoiDem CHECK (GiaMoiDem >= 0),
    CONSTRAINT CK_ChiTietDatPhong_CheckOut
        CHECK (
            NgayCheckOutThucTe IS NULL
            OR NgayCheckInThucTe IS NULL
            OR NgayCheckOutThucTe >= NgayCheckInThucTe
        )
);
GO

/* 7. Payments.
      No UNIQUE(MaDatPhong): one booking may have multiple payment attempts/records.
*/
CREATE TABLE dbo.ThanhToan (
    MaThanhToan INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ThanhToan PRIMARY KEY,
    MaDatPhong INT NOT NULL,
    PhuongThucThanhToan NVARCHAR(100) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    TrangThaiThanhToan NVARCHAR(50) NOT NULL,
    MaGiaoDich NVARCHAR(150) NULL,
    NgayThanhToan DATETIME2 NULL,
    CONSTRAINT FK_ThanhToan_DatPhong
        FOREIGN KEY (MaDatPhong)
        REFERENCES dbo.DatPhong(MaDatPhong),
    CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien >= 0)
);
GO

/* 8. Reviews. */
CREATE TABLE dbo.DanhGia (
    MaDanhGia INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_DanhGia PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaNoiLuuTru INT NOT NULL,
    DiemDanhGia TINYINT NOT NULL,
    NoiDung NVARCHAR(1000) NULL,
    NgayTao DATETIME2 NOT NULL
        CONSTRAINT DF_DanhGia_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_DanhGia_NguoiDung
        FOREIGN KEY (MaNguoiDung)
        REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DanhGia_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru)
        REFERENCES dbo.NoiLuuTru(MaNoiLuuTru),
    CONSTRAINT UQ_DanhGia_NguoiDung_NoiLuuTru
        UNIQUE (MaNguoiDung, MaNoiLuuTru),
    CONSTRAINT CK_DanhGia_Diem CHECK (DiemDanhGia BETWEEN 1 AND 5)
);
GO

/* Helpful indexes for common booking lookups. */
CREATE INDEX IX_DatPhong_MaNguoiDung
    ON dbo.DatPhong(MaNguoiDung);

CREATE INDEX IX_DatPhong_MaNhanVienTao
    ON dbo.DatPhong(MaNhanVienTao);

CREATE INDEX IX_ChiTietDatPhong_MaPhong
    ON dbo.ChiTietDatPhong(MaPhong);

CREATE INDEX IX_ThanhToan_MaDatPhong
    ON dbo.ThanhToan(MaDatPhong);
GO

PRINT N'Khoi tao co so du lieu StayBook thanh cong.';
GO
