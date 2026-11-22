IF DB_ID(N'StayBook') IS NULL
BEGIN
    CREATE DATABASE StayBook;
END;
GO

USE StayBook;
GO

IF OBJECT_ID(N'dbo.NguoiDung', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.NoiLuuTru', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.DatPhong', N'U') IS NOT NULL
   OR OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL
BEGIN
    RAISERROR (N'StayBook already contains tables. Back up/migrate the existing database or use a new empty database before running this initialization script.', 16, 1);
    RETURN;
END;
GO

CREATE TABLE dbo.NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_NguoiDung PRIMARY KEY,
    HoTen NVARCHAR(150) NOT NULL,
    UserName NVARCHAR(256) NOT NULL,
    NormalizedUserName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    NormalizedEmail NVARCHAR(256) NOT NULL,
    EmailConfirmed BIT NOT NULL
        CONSTRAINT DF_NguoiDung_EmailConfirmed DEFAULT (0),
    MatKhauHash NVARCHAR(MAX) NOT NULL,
    SecurityStamp NVARCHAR(MAX) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL,
    SoDT NVARCHAR(MAX) NULL,
    PhoneNumberConfirmed BIT NOT NULL
        CONSTRAINT DF_NguoiDung_PhoneNumberConfirmed DEFAULT (0),
    TwoFactorEnabled BIT NOT NULL
        CONSTRAINT DF_NguoiDung_TwoFactorEnabled DEFAULT (0),
    LockoutEnd DATETIMEOFFSET NULL,
    LockoutEnabled BIT NOT NULL
        CONSTRAINT DF_NguoiDung_LockoutEnabled DEFAULT (0),
    AccessFailedCount INT NOT NULL
        CONSTRAINT DF_NguoiDung_AccessFailedCount DEFAULT (0),
    TrangThaiHoatDong BIT NOT NULL
        CONSTRAINT DF_NguoiDung_TrangThaiHoatDong DEFAULT (1),
    VaiTro NVARCHAR(30) NOT NULL
        CONSTRAINT DF_NguoiDung_VaiTro DEFAULT (N'KhachHang'),
    NgayTao DATETIME2(0) NOT NULL
        CONSTRAINT DF_NguoiDung_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT UQ_NguoiDung_Email UNIQUE (Email),
    CONSTRAINT CK_NguoiDung_VaiTro
        CHECK (VaiTro IN (N'KhachHang', N'NhanVien', N'QuanTriVien')),
    CONSTRAINT CK_NguoiDung_AccessFailedCount CHECK (AccessFailedCount >= 0)
);
GO

CREATE UNIQUE INDEX UX_NguoiDung_NormalizedUserName
    ON dbo.NguoiDung(NormalizedUserName);
CREATE INDEX IX_NguoiDung_NormalizedEmail
    ON dbo.NguoiDung(NormalizedEmail);
CREATE INDEX IX_NguoiDung_VaiTro_TrangThai
    ON dbo.NguoiDung(VaiTro, TrangThaiHoatDong);
GO

CREATE TABLE dbo.AspNetRoles (
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_AspNetRoles PRIMARY KEY,
    Name NVARCHAR(256) NULL,
    NormalizedName NVARCHAR(256) NULL,
    ConcurrencyStamp NVARCHAR(MAX) NULL
);
GO
CREATE UNIQUE INDEX RoleNameIndex
    ON dbo.AspNetRoles(NormalizedName)
    WHERE NormalizedName IS NOT NULL;
GO

CREATE TABLE dbo.AspNetUserClaims (
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_AspNetUserClaims PRIMARY KEY,
    UserId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetUserClaims_NguoiDung
        FOREIGN KEY (UserId) REFERENCES dbo.NguoiDung(MaNguoiDung)
        ON DELETE CASCADE
);
GO
CREATE INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims(UserId);
GO

CREATE TABLE dbo.AspNetUserLogins (
    LoginProvider NVARCHAR(128) NOT NULL,
    ProviderKey NVARCHAR(128) NOT NULL,
    ProviderDisplayName NVARCHAR(MAX) NULL,
    UserId INT NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_NguoiDung
        FOREIGN KEY (UserId) REFERENCES dbo.NguoiDung(MaNguoiDung)
        ON DELETE CASCADE
);
GO
CREATE INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins(UserId);
GO

CREATE TABLE dbo.AspNetUserTokens (
    UserId INT NOT NULL,
    LoginProvider NVARCHAR(128) NOT NULL,
    Name NVARCHAR(128) NOT NULL,
    Value NVARCHAR(MAX) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_NguoiDung
        FOREIGN KEY (UserId) REFERENCES dbo.NguoiDung(MaNguoiDung)
        ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AspNetRoleClaims (
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY,
    RoleId INT NOT NULL,
    ClaimType NVARCHAR(MAX) NULL,
    ClaimValue NVARCHAR(MAX) NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles
        FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id)
        ON DELETE CASCADE
);
GO
CREATE INDEX IX_AspNetRoleClaims_RoleId ON dbo.AspNetRoleClaims(RoleId);
GO

CREATE TABLE dbo.AspNetUserRoles (
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_NguoiDung
        FOREIGN KEY (UserId) REFERENCES dbo.NguoiDung(MaNguoiDung)
        ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles
        FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id)
        ON DELETE CASCADE
);
GO
CREATE UNIQUE INDEX UX_AspNetUserRoles_OneRolePerUser
    ON dbo.AspNetUserRoles(UserId);
CREATE INDEX IX_AspNetUserRoles_RoleId
    ON dbo.AspNetUserRoles(RoleId);
GO

CREATE TRIGGER dbo.TR_AspNetUserRoles_SyncVaiTro
ON dbo.AspNetUserRoles
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
       SET u.VaiTro = r.Name
    FROM dbo.NguoiDung AS u
    INNER JOIN inserted AS i ON i.UserId = u.MaNguoiDung
    INNER JOIN dbo.AspNetRoles AS r ON r.Id = i.RoleId;
END;
GO

CREATE TRIGGER dbo.TR_AspNetUserRoles_ResetVaiTroAfterDelete
ON dbo.AspNetUserRoles
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE u
       SET u.VaiTro = N'KhachHang'
    FROM dbo.NguoiDung AS u
    INNER JOIN deleted AS d ON d.UserId = u.MaNguoiDung
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.AspNetUserRoles AS ur
        WHERE ur.UserId = u.MaNguoiDung
    );
END;
GO

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
        FOREIGN KEY (MaNoiLuuTru) REFERENCES dbo.NoiLuuTru(MaNoiLuuTru)
);
GO

CREATE TABLE dbo.LoaiPhong (
    MaLoaiPhong INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_LoaiPhong PRIMARY KEY,
    MaNoiLuuTru INT NOT NULL,
    TenLoaiPhong NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(MAX) NULL,
    SucChua INT NOT NULL,
    GiaCoBan DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_LoaiPhong_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES dbo.NoiLuuTru(MaNoiLuuTru),
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
        FOREIGN KEY (MaLoaiPhong) REFERENCES dbo.LoaiPhong(MaLoaiPhong),
    CONSTRAINT UQ_Phong_LoaiPhong_SoPhong UNIQUE (MaLoaiPhong, SoPhong)
);
GO

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
    CONSTRAINT PK_LoaiPhong_TienNghi PRIMARY KEY (MaLoaiPhong, MaTienNghi),
    CONSTRAINT FK_LPTN_LoaiPhong
        FOREIGN KEY (MaLoaiPhong) REFERENCES dbo.LoaiPhong(MaLoaiPhong),
    CONSTRAINT FK_LPTN_TienNghi
        FOREIGN KEY (MaTienNghi) REFERENCES dbo.TienNghi(MaTienNghi)
);
GO

CREATE TABLE dbo.DatPhong (
    MaDatPhong INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_DatPhong PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaNhanVienTao INT NULL,
    MaNoiLuuTru INT NOT NULL,
    NgayNhanPhong DATE NOT NULL,
    NgayTraPhong DATE NOT NULL,
    SoLuongKhach INT NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThaiDatPhong NVARCHAR(50) NOT NULL,
    NgayTao DATETIME2(0) NOT NULL
        CONSTRAINT DF_DatPhong_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_DatPhong_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DatPhong_NhanVienTao
        FOREIGN KEY (MaNhanVienTao) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DatPhong_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES dbo.NoiLuuTru(MaNoiLuuTru),
    CONSTRAINT CK_DatPhong_Ngay CHECK (NgayTraPhong > NgayNhanPhong),
    CONSTRAINT CK_DatPhong_SoLuongKhach CHECK (SoLuongKhach > 0),
    CONSTRAINT CK_DatPhong_TongTien CHECK (TongTien >= 0)
);
GO

CREATE TRIGGER dbo.TR_DatPhong_ValidateNhanVienTao
ON dbo.DatPhong
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        LEFT JOIN dbo.NguoiDung AS creator
            ON creator.MaNguoiDung = i.MaNhanVienTao
        WHERE i.MaNhanVienTao IS NOT NULL
          AND (creator.MaNguoiDung IS NULL
               OR creator.VaiTro NOT IN (N'NhanVien', N'QuanTriVien'))
    )
    BEGIN
        RAISERROR (N'MaNhanVienTao phải thuộc tài khoản có vai trò NhanVien hoặc QuanTriVien.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

CREATE TABLE dbo.ChiTietDatPhong (
    MaDatPhong INT NOT NULL,
    MaPhong INT NOT NULL,
    GiaMoiDem DECIMAL(18,2) NOT NULL,
    TrangThaiNhanPhong NVARCHAR(50) NULL,
    TrangThaiTraPhong NVARCHAR(50) NULL,
    NgayCheckInThucTe DATETIME2(0) NULL,
    NgayCheckOutThucTe DATETIME2(0) NULL,
    CONSTRAINT PK_ChiTietDatPhong PRIMARY KEY (MaDatPhong, MaPhong),
    CONSTRAINT FK_ChiTietDatPhong_DatPhong
        FOREIGN KEY (MaDatPhong) REFERENCES dbo.DatPhong(MaDatPhong),
    CONSTRAINT FK_ChiTietDatPhong_Phong
        FOREIGN KEY (MaPhong) REFERENCES dbo.Phong(MaPhong),
    CONSTRAINT CK_ChiTietDatPhong_GiaMoiDem CHECK (GiaMoiDem >= 0),
    CONSTRAINT CK_ChiTietDatPhong_CheckOut
        CHECK (NgayCheckOutThucTe IS NULL
               OR NgayCheckInThucTe IS NULL
               OR NgayCheckOutThucTe >= NgayCheckInThucTe)
);
GO

CREATE TRIGGER dbo.TR_ChiTietDatPhong_ValidateNoiLuuTru
ON dbo.ChiTietDatPhong
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted AS i
        INNER JOIN dbo.DatPhong AS dp ON dp.MaDatPhong = i.MaDatPhong
        INNER JOIN dbo.Phong AS p ON p.MaPhong = i.MaPhong
        INNER JOIN dbo.LoaiPhong AS lp ON lp.MaLoaiPhong = p.MaLoaiPhong
        WHERE dp.MaNoiLuuTru <> lp.MaNoiLuuTru
    )
    BEGIN
        RAISERROR (N'Tất cả phòng trong cùng một đơn phải thuộc cùng nơi lưu trú của đơn.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

CREATE TABLE dbo.ThanhToan (
    MaThanhToan INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_ThanhToan PRIMARY KEY,
    MaDatPhong INT NOT NULL,
    PhuongThucThanhToan NVARCHAR(100) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    TrangThaiThanhToan NVARCHAR(50) NOT NULL,
    MaGiaoDich NVARCHAR(150) NULL,
    NgayThanhToan DATETIME2(0) NULL,
    CONSTRAINT FK_ThanhToan_DatPhong
        FOREIGN KEY (MaDatPhong) REFERENCES dbo.DatPhong(MaDatPhong),
    CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien >= 0)
);
GO

CREATE TABLE dbo.DanhGia (
    MaDanhGia INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_DanhGia PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaNoiLuuTru INT NOT NULL,
    DiemDanhGia TINYINT NOT NULL,
    NoiDung NVARCHAR(1000) NULL,
    NgayTao DATETIME2(0) NOT NULL
        CONSTRAINT DF_DanhGia_NgayTao DEFAULT (SYSDATETIME()),
    CONSTRAINT FK_DanhGia_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES dbo.NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DanhGia_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES dbo.NoiLuuTru(MaNoiLuuTru),
    CONSTRAINT UQ_DanhGia_NguoiDung_NoiLuuTru UNIQUE (MaNguoiDung, MaNoiLuuTru),
    CONSTRAINT CK_DanhGia_Diem CHECK (DiemDanhGia BETWEEN 1 AND 5)
);
GO

CREATE INDEX IX_HinhAnhNoiLuuTru_MaNoiLuuTru ON dbo.HinhAnhNoiLuuTru(MaNoiLuuTru);
CREATE INDEX IX_LoaiPhong_MaNoiLuuTru ON dbo.LoaiPhong(MaNoiLuuTru);
CREATE INDEX IX_LoaiPhong_GiaCoBan_SucChua ON dbo.LoaiPhong(GiaCoBan, SucChua);
CREATE INDEX IX_Phong_MaLoaiPhong_TrangThai ON dbo.Phong(MaLoaiPhong, TrangThaiPhong);
CREATE INDEX IX_LPTN_MaTienNghi ON dbo.LoaiPhong_TienNghi(MaTienNghi);
CREATE INDEX IX_DatPhong_MaNguoiDung ON dbo.DatPhong(MaNguoiDung);
CREATE INDEX IX_DatPhong_MaNhanVienTao ON dbo.DatPhong(MaNhanVienTao);
CREATE INDEX IX_DatPhong_MaNoiLuuTru_Ngay ON dbo.DatPhong(MaNoiLuuTru, NgayNhanPhong, NgayTraPhong, TrangThaiDatPhong);
CREATE INDEX IX_ChiTietDatPhong_MaPhong ON dbo.ChiTietDatPhong(MaPhong);
CREATE INDEX IX_ThanhToan_MaDatPhong ON dbo.ThanhToan(MaDatPhong);
CREATE INDEX IX_ThanhToan_TrangThai ON dbo.ThanhToan(TrangThaiThanhToan);
CREATE INDEX IX_DanhGia_MaNoiLuuTru ON dbo.DanhGia(MaNoiLuuTru);
GO

DECLARE @DemoPasswordHash NVARCHAR(MAX) = N'AQAAAAIAAYagAAAAEGkbLBE+1iPV+Wa2CS4EXinWoiF0Urd1ETv5FlfA6DMQA1HUCUw5fPUmU1LA77kx9w==';

INSERT INTO dbo.NguoiDung
(
    HoTen, UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, MatKhauHash, SecurityStamp, ConcurrencyStamp,
    SoDT, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled,
    AccessFailedCount, TrangThaiHoatDong, VaiTro
)
VALUES
(N'Nguyễn Văn An', N'an.nguyen@gmail.com', N'AN.NGUYEN@GMAIL.COM', N'an.nguyen@gmail.com', N'AN.NGUYEN@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0901234567', 0, 0, 0, 0, 1, N'KhachHang'),
(N'Trần Thị Bích', N'bich.tran@gmail.com', N'BICH.TRAN@GMAIL.COM', N'bich.tran@gmail.com', N'BICH.TRAN@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0912345678', 0, 0, 0, 0, 1, N'KhachHang'),
(N'Lê Hoàng Cường', N'cuong.le@gmail.com', N'CUONG.LE@GMAIL.COM', N'cuong.le@gmail.com', N'CUONG.LE@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0923456789', 0, 0, 0, 0, 1, N'KhachHang'),
(N'Phạm Thị Dung', N'dung.pham@gmail.com', N'DUNG.PHAM@GMAIL.COM', N'dung.pham@gmail.com', N'DUNG.PHAM@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0934567890', 0, 0, 0, 0, 1, N'KhachHang'),
(N'Hoàng Văn Em', N'em.hoang@gmail.com', N'EM.HOANG@GMAIL.COM', N'em.hoang@gmail.com', N'EM.HOANG@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0945678901', 0, 0, 0, 0, 1, N'NhanVien'),
(N'Vũ Thị Phương', N'phuong.vu@gmail.com', N'PHUONG.VU@GMAIL.COM', N'phuong.vu@gmail.com', N'PHUONG.VU@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0956789012', 0, 0, 0, 0, 1, N'NhanVien'),
(N'Đặng Văn Giang', N'giang.dang@gmail.com', N'GIANG.DANG@GMAIL.COM', N'giang.dang@gmail.com', N'GIANG.DANG@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0967890123', 0, 0, 0, 0, 1, N'KhachHang'),
(N'Bùi Thị Hương', N'huong.bui@gmail.com', N'HUONG.BUI@GMAIL.COM', N'huong.bui@gmail.com', N'HUONG.BUI@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0978901234', 0, 0, 0, 0, 1, N'KhachHang'),
(N'Ngô Văn Hùng', N'hung.ngo@gmail.com', N'HUNG.NGO@GMAIL.COM', N'hung.ngo@gmail.com', N'HUNG.NGO@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0989012345', 0, 0, 0, 0, 1, N'QuanTriVien'),
(N'Dương Thị Linh', N'linh.duong@gmail.com', N'LINH.DUONG@GMAIL.COM', N'linh.duong@gmail.com', N'LINH.DUONG@GMAIL.COM', 1, @DemoPasswordHash, CONVERT(NVARCHAR(36), NEWID()), CONVERT(NVARCHAR(36), NEWID()), N'0990123456', 0, 0, 0, 0, 1, N'KhachHang');
GO

INSERT INTO dbo.AspNetRoles (Name, NormalizedName, ConcurrencyStamp)
VALUES
(N'KhachHang', N'KHACHHANG', CONVERT(NVARCHAR(36), NEWID())),
(N'NhanVien', N'NHANVIEN', CONVERT(NVARCHAR(36), NEWID())),
(N'QuanTriVien', N'QUANTRIVIEN', CONVERT(NVARCHAR(36), NEWID()));
GO

INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
SELECT u.MaNguoiDung, r.Id
FROM dbo.NguoiDung AS u
INNER JOIN dbo.AspNetRoles AS r
    ON r.Name = u.VaiTro;
GO

INSERT INTO dbo.NoiLuuTru (TenNoiLuuTru, LoaiHinh, DiaChi, MoTa, TrangThaiHoatDong)
VALUES
(N'Sunset Beach Resort', N'Resort', N'123 Đường Trần Phú, Nha Trang, Khánh Hòa', N'Khu nghỉ dưỡng sát biển với view đẹp.', 1),
(N'Green Valley Homestay', N'Homestay', N'45 Nguyễn Chí Thanh, Đà Lạt, Lâm Đồng', N'Homestay phong cách mộc mạc giữa thung lũng thông.', 1),
(N'Grand Luxury Hotel', N'Khách sạn', N'10 Lê Duẩn, Quận 1, TP. Hồ Chí Minh', N'Khách sạn sang trọng giữa trung tâm thành phố.', 1),
(N'Cozy Corner Apartment', N'Căn hộ', N'88 Phố Huế, Hoàn Kiếm, Hà Nội', N'Căn hộ ấm cúng, phù hợp cho gia đình.', 1),
(N'Seaview Villa Vũng Tàu', N'Villa', N'12 Hạ Long, Phường 2, Vũng Tàu', N'Villa có hồ bơi riêng nhìn ra biển.', 1),
(N'Mountain Wind Homestay', N'Homestay', N'Thôn Bản Là, Sa Pa, Lào Cai', N'Trải nghiệm không gian bản địa vùng cao.', 1),
(N'Riverside Boutique Hotel', N'Khách sạn', N'56 Bạch Đằng, Hải Châu, Đà Nẵng', N'Khách sạn boutique cạnh sông Hàn.', 1),
(N'Pine Hill Cottage', N'Homestay', N'Đường Đống Đa, Đà Lạt, Lâm Đồng', N'Nhà gỗ nhỏ dưới đồi thông.', 1),
(N'Central Park Hotel', N'Khách sạn', N'20 Hùng Vương, Thành phố Huế', N'Khách sạn gần khu di tích cố đô.', 1),
(N'Tropical Oasis Resort', N'Resort', N'Bãi Trường, Dương Tơ, Phú Quốc', N'Resort nhiệt đới cho kỳ nghỉ dưỡng.', 1);
GO

INSERT INTO dbo.HinhAnhNoiLuuTru (MaNoiLuuTru, DuongDanAnh, LaAnhDaiDien)
VALUES
(1, N'/images/sunset_1.jpg', 1), (2, N'/images/green_1.jpg', 1),
(3, N'/images/grand_1.jpg', 1), (4, N'/images/cozy_1.jpg', 1),
(5, N'/images/villa_1.jpg', 1), (6, N'/images/mountain_1.jpg', 1),
(7, N'/images/riverside_1.jpg', 1), (8, N'/images/pine_1.jpg', 1),
(9, N'/images/central_1.jpg', 1), (10, N'/images/tropical_1.jpg', 1);
GO

INSERT INTO dbo.LoaiPhong (MaNoiLuuTru, TenLoaiPhong, MoTa, SucChua, GiaCoBan)
VALUES
(1, N'Phòng Deluxe Hướng Biển', N'Phòng 35m2 có ban công nhìn ra biển.', 2, 1200000),
(1, N'Phòng Suite Gia Đình', N'Phòng rộng, có hai giường lớn.', 4, 2500000),
(2, N'Phòng Đôi Tiêu Chuẩn', N'Phòng ấm cúng, view đồi thông.', 2, 500000),
(3, N'Phòng Superior Twin', N'Phòng hai giường đơn hiện đại.', 2, 1800000),
(3, N'Phòng Presidential Suite', N'Phòng cao cấp ở tầng trên.', 2, 5500000),
(4, N'Căn Hộ 1 Phòng Ngủ', N'Căn hộ có bếp và máy giặt.', 2, 800000),
(5, N'Villa Nguyên Căn 3 Phòng Ngủ', N'Villa có hồ bơi và sân vườn.', 8, 4500000),
(6, N'Phòng Tập Thể Dorm', N'Giường tầng, phù hợp khách đi một mình.', 1, 200000),
(7, N'Phòng Deluxe Sông Hàn', N'Phòng có tầm nhìn ra sông Hàn.', 2, 1500000),
(10, N'Bungalow Sát Biển', N'Bungalow riêng tư gần biển.', 2, 2200000);
GO

INSERT INTO dbo.Phong (MaLoaiPhong, SoPhong, TrangThaiPhong)
VALUES
(1, N'101', N'Trống'),
(1, N'102', N'Trống'),
(2, N'201', N'Trống'),
(3, N'301', N'Trống'),
(4, N'401', N'Trống'),
(5, N'501', N'Trống'),
(7, N'V01', N'Trống'),
(8, N'D01', N'Trống'),
(9, N'901', N'Trống'),
(10, N'B01', N'Trống'),
(6, N'A101', N'Trống'),
(6, N'A102', N'Trống');
GO

INSERT INTO dbo.TienNghi (TenTienNghi, MoTa)
VALUES
(N'Wi-Fi miễn phí', N'Internet tốc độ cao.'),
(N'Hồ bơi', N'Hồ bơi ngoài trời.'),
(N'Điều hòa không khí', N'Máy điều hòa hai chiều.'),
(N'Bãi đỗ xe', N'Bãi đỗ xe an toàn.'),
(N'Bữa sáng miễn phí', N'Phục vụ bữa sáng.'),
(N'Dịch vụ phòng 24/7', N'Phục vụ theo yêu cầu.'),
(N'Bếp nấu ăn', N'Có dụng cụ nấu ăn cơ bản.'),
(N'Máy giặt', N'Máy giặt trong căn hộ.'),
(N'Tivi màn hình phẳng', N'Tivi kết nối internet.'),
(N'Ban công/Sân hiên', N'Không gian thư giãn ngoài trời.');
GO

INSERT INTO dbo.LoaiPhong_TienNghi (MaLoaiPhong, MaTienNghi)
VALUES
(1,1),(1,3),(1,5),(1,9),(1,10),
(2,1),(2,2),(2,3),(2,5),(2,9),
(3,1),(3,3),(4,1),(4,3),(5,1),
(6,1),(6,7),(6,8),(7,1),(7,2),
(8,1),(9,1),(9,3),(10,1),(10,10);
GO

INSERT INTO dbo.DatPhong
(MaNguoiDung, MaNhanVienTao, MaNoiLuuTru, NgayNhanPhong, NgayTraPhong, SoLuongKhach, TongTien, TrangThaiDatPhong)
VALUES
(1, 5, 1, '2027-01-10', '2027-01-13', 4, 7200000, N'Đã xác nhận'),
(2, NULL, 2, '2027-01-15', '2027-01-17', 2, 1000000, N'Đã xác nhận'),
(3, NULL, 3, '2026-07-01', '2026-07-04', 2, 5400000, N'Hoàn thành'),
(4, NULL, 4, '2026-07-05', '2026-07-07', 2, 1600000, N'Đã hủy'),
(7, 6, 5, '2027-02-10', '2027-02-13', 6, 13500000, N'Đã xác nhận'),
(8, NULL, 6, '2026-08-15', '2026-08-16', 1, 200000, N'Hoàn thành'),
(10, NULL, 7, '2027-03-02', '2027-03-04', 2, 3000000, N'Đã xác nhận'),
(1, NULL, 1, '2027-03-10', '2027-03-12', 4, 5000000, N'Đã xác nhận'),
(2, NULL, 7, '2026-10-01', '2026-10-03', 2, 3000000, N'Hoàn thành'),
(3, NULL, 10, '2027-04-10', '2027-04-13', 2, 6600000, N'Chờ xác nhận'),
(1, NULL, 1, '2026-09-10', '2026-09-12', 4, 5000000, N'Hoàn thành'),
(4, NULL, 4, '2026-06-10', '2026-06-12', 2, 1600000, N'Hoàn thành'),
(7, NULL, 5, '2026-06-01', '2026-06-03', 6, 9000000, N'Hoàn thành'),
(10, NULL, 7, '2026-05-02', '2026-05-04', 2, 3000000, N'Hoàn thành'),
(3, NULL, 10, '2026-05-20', '2026-05-23', 2, 6600000, N'Hoàn thành');
GO

INSERT INTO dbo.ChiTietDatPhong
(MaDatPhong, MaPhong, GiaMoiDem, TrangThaiNhanPhong, TrangThaiTraPhong, NgayCheckInThucTe, NgayCheckOutThucTe)
VALUES
(1, 1, 1200000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(1, 2, 1200000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(2, 4, 500000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(3, 5, 1800000, N'Đã nhận', N'Đã trả', '2026-07-01 14:00:00', '2026-07-04 11:30:00'),
(4, 11, 800000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(5, 7, 4500000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(6, 8, 200000, N'Đã nhận', N'Đã trả', '2026-08-15 15:00:00', '2026-08-16 10:00:00'),
(7, 9, 1500000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(8, 3, 2500000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(9, 9, 1500000, N'Đã nhận', N'Đã trả', '2026-10-01 14:00:00', '2026-10-03 12:00:00'),
(10, 10, 2200000, N'Chưa nhận', N'Chưa trả', NULL, NULL),
(11, 3, 2500000, N'Đã nhận', N'Đã trả', '2026-09-10 14:00:00', '2026-09-12 11:00:00'),
(12, 12, 800000, N'Đã nhận', N'Đã trả', '2026-06-10 14:00:00', '2026-06-12 11:00:00'),
(13, 7, 4500000, N'Đã nhận', N'Đã trả', '2026-06-01 14:00:00', '2026-06-03 11:00:00'),
(14, 9, 1500000, N'Đã nhận', N'Đã trả', '2026-05-02 14:00:00', '2026-05-04 11:00:00'),
(15, 10, 2200000, N'Đã nhận', N'Đã trả', '2026-05-20 14:00:00', '2026-05-23 11:00:00');
GO

INSERT INTO dbo.ThanhToan
(MaDatPhong, PhuongThucThanhToan, SoTien, TrangThaiThanhToan, MaGiaoDich, NgayThanhToan)
VALUES
(1, N'Chuyển khoản ngân hàng', 3600000, N'Thất bại', N'TXN001-A', '2027-01-09 10:00:00'),
(1, N'Chuyển khoản ngân hàng', 7200000, N'Đã thanh toán', N'TXN001-B', '2027-01-09 10:15:00'),
(2, N'Thẻ tín dụng', 1000000, N'Đã thanh toán', N'TXN002', '2027-01-14 09:00:00'),
(3, N'Tiền mặt', 5400000, N'Đã thanh toán', N'TXN003', '2026-07-01 13:00:00'),
(4, N'Ví điện tử', 1600000, N'Đã hoàn tiền', N'TXN004', '2026-07-04 08:00:00'),
(5, N'Chuyển khoản ngân hàng', 13500000, N'Đã thanh toán', N'TXN005', '2027-02-09 14:00:00'),
(6, N'Tiền mặt', 200000, N'Đã thanh toán', N'TXN006', '2026-08-15 14:00:00'),
(7, N'Thẻ tín dụng', 3000000, N'Chờ thanh toán', NULL, NULL),
(8, N'Ví điện tử', 5000000, N'Đã thanh toán', N'TXN008', '2027-03-09 10:00:00'),
(9, N'Chuyển khoản ngân hàng', 3000000, N'Đã thanh toán', N'TXN009', '2026-09-30 09:00:00'),
(10, N'Thẻ tín dụng', 6600000, N'Chờ thanh toán', NULL, NULL),
(11, N'Tiền mặt', 5000000, N'Đã thanh toán', N'TXN011', '2026-09-10 13:00:00'),
(12, N'Tiền mặt', 1600000, N'Đã thanh toán', N'TXN012', '2026-06-10 13:00:00'),
(13, N'Chuyển khoản ngân hàng', 9000000, N'Đã thanh toán', N'TXN013', '2026-05-31 12:00:00'),
(14, N'Thẻ tín dụng', 3000000, N'Đã thanh toán', N'TXN014', '2026-05-01 12:00:00'),
(15, N'Chuyển khoản ngân hàng', 6600000, N'Đã thanh toán', N'TXN015', '2026-05-19 12:00:00');
GO

INSERT INTO dbo.DanhGia (MaNguoiDung, MaNoiLuuTru, DiemDanhGia, NoiDung)
VALUES
(3, 3, 5, N'Khách sạn rất tuyệt vời, nhân viên thân thiện, phòng sạch sẽ.'),
(8, 6, 4, N'Homestay ấm cúng, cảnh đẹp và yên tĩnh.'),
(2, 7, 5, N'Vị trí thuận tiện, phòng sạch sẽ.'),
(1, 1, 5, N'Resort sát biển đẹp, dịch vụ tốt.'),
(4, 4, 3, N'Căn hộ đầy đủ tiện nghi, phù hợp gia đình.'),
(7, 5, 5, N'Villa rộng rãi, hồ bơi sạch sẽ.'),
(10, 7, 4, N'Khách sạn có tầm nhìn đẹp.'),
(3, 10, 5, N'Bungalow gần biển, không gian riêng tư.');
GO

PRINT N'Khoi tao co so du lieu StayBook va du lieu mau thanh cong.';
GO
