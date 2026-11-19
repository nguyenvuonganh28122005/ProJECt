/*
    STAYBOOK - MySQL 8.0.16+
    Full initialization script: 11 tables + sample data.
    IMPORTANT:
    - Run this script on a NEW/EMPTY database.
    - It does not drop existing databases/tables or delete existing data.
    - Sample account passwords below are placeholders, not production password hashes.
*/

CREATE DATABASE IF NOT EXISTS StayBook
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;
USE StayBook;

-- 1. Người dùng: KhachHang, NhanVien, QuanTriVien
CREATE TABLE NguoiDung (
    MaNguoiDung INT AUTO_INCREMENT PRIMARY KEY,
    HoTen VARCHAR(150) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    SoDT VARCHAR(20) NULL,
    MatKhauHash VARCHAR(500) NOT NULL,
    TrangThaiHoatDong BOOLEAN NOT NULL DEFAULT TRUE,
    VaiTro VARCHAR(30) NOT NULL,
    NgayTao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_NguoiDung_Email UNIQUE (Email),
    CONSTRAINT CK_NguoiDung_VaiTro
        CHECK (VaiTro IN ('KhachHang', 'NhanVien', 'QuanTriVien')),
    INDEX idx_nguoidung_trangthai (TrangThaiHoatDong)
) ENGINE=InnoDB;

-- 2. Nơi lưu trú
CREATE TABLE NoiLuuTru (
    MaNoiLuuTru INT AUTO_INCREMENT PRIMARY KEY,
    TenNoiLuuTru VARCHAR(200) NOT NULL,
    LoaiHinh VARCHAR(100) NOT NULL,
    DiaChi VARCHAR(300) NOT NULL,
    MoTa TEXT NULL,
    TrangThaiHoatDong BOOLEAN NOT NULL DEFAULT TRUE,
    INDEX idx_noiluutru_loaihinh (LoaiHinh),
    INDEX idx_noiluutru_trangthai (TrangThaiHoatDong)
) ENGINE=InnoDB;

-- 3. Hình ảnh nơi lưu trú
CREATE TABLE HinhAnhNoiLuuTru (
    MaHinhAnh INT AUTO_INCREMENT PRIMARY KEY,
    MaNoiLuuTru INT NOT NULL,
    DuongDanAnh VARCHAR(500) NOT NULL,
    LaAnhDaiDien BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT FK_HinhAnhNoiLuuTru_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES NoiLuuTru(MaNoiLuuTru)
        ON DELETE CASCADE,
    INDEX idx_hinhanh_noiluutru (MaNoiLuuTru)
) ENGINE=InnoDB;

-- 4. Loại phòng
CREATE TABLE LoaiPhong (
    MaLoaiPhong INT AUTO_INCREMENT PRIMARY KEY,
    MaNoiLuuTru INT NOT NULL,
    TenLoaiPhong VARCHAR(150) NOT NULL,
    MoTa TEXT NULL,
    SucChua INT NOT NULL,
    GiaCoBan DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_LoaiPhong_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES NoiLuuTru(MaNoiLuuTru)
        ON DELETE CASCADE,
    CONSTRAINT CK_LoaiPhong_SucChua CHECK (SucChua > 0),
    CONSTRAINT CK_LoaiPhong_GiaCoBan CHECK (GiaCoBan >= 0),
    INDEX idx_loaiphong_noiluutru (MaNoiLuuTru),
    INDEX idx_loaiphong_giaban (GiaCoBan),
    INDEX idx_loaiphong_succhua (SucChua)
) ENGINE=InnoDB;

-- 5. Phòng vật lý
CREATE TABLE Phong (
    MaPhong INT AUTO_INCREMENT PRIMARY KEY,
    MaLoaiPhong INT NOT NULL,
    SoPhong VARCHAR(50) NOT NULL,
    TrangThaiPhong VARCHAR(50) NOT NULL,
    CONSTRAINT FK_Phong_LoaiPhong
        FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhong(MaLoaiPhong)
        ON DELETE CASCADE,
    CONSTRAINT UQ_Phong_LoaiPhong_SoPhong UNIQUE (MaLoaiPhong, SoPhong),
    INDEX idx_phong_trangthai (TrangThaiPhong)
) ENGINE=InnoDB;

-- 6. Tiện nghi
CREATE TABLE TienNghi (
    MaTienNghi INT AUTO_INCREMENT PRIMARY KEY,
    TenTienNghi VARCHAR(100) NOT NULL,
    MoTa TEXT NULL
) ENGINE=InnoDB;

-- 7. Quan hệ nhiều-nhiều LoaiPhong - TienNghi
CREATE TABLE LoaiPhong_TienNghi (
    MaLoaiPhong INT NOT NULL,
    MaTienNghi INT NOT NULL,
    PRIMARY KEY (MaLoaiPhong, MaTienNghi),
    CONSTRAINT FK_LPTN_LoaiPhong
        FOREIGN KEY (MaLoaiPhong) REFERENCES LoaiPhong(MaLoaiPhong)
        ON DELETE CASCADE,
    CONSTRAINT FK_LPTN_TienNghi
        FOREIGN KEY (MaTienNghi) REFERENCES TienNghi(MaTienNghi)
        ON DELETE CASCADE
) ENGINE=InnoDB;

-- 8. Đặt phòng.
-- MaNguoiDung là khách sở hữu đơn; MaNhanVienTao NULL nếu khách tự đặt.
-- MaNoiLuuTru giữ thông tin nơi lưu trú của đơn để lọc/tổng hợp thuận tiện.
CREATE TABLE DatPhong (
    MaDatPhong INT AUTO_INCREMENT PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaNhanVienTao INT NULL,
    MaNoiLuuTru INT NOT NULL,
    NgayNhanPhong DATE NOT NULL,
    NgayTraPhong DATE NOT NULL,
    SoLuongKhach INT NOT NULL,
    TongTien DECIMAL(18,2) NOT NULL,
    TrangThaiDatPhong VARCHAR(50) NOT NULL,
    NgayTao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_DatPhong_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DatPhong_NhanVienTao
        FOREIGN KEY (MaNhanVienTao) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DatPhong_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES NoiLuuTru(MaNoiLuuTru),
    CONSTRAINT CK_DatPhong_Ngay CHECK (NgayTraPhong > NgayNhanPhong),
    CONSTRAINT CK_DatPhong_SoLuongKhach CHECK (SoLuongKhach > 0),
    CONSTRAINT CK_DatPhong_TongTien CHECK (TongTien >= 0),
    INDEX idx_datphong_nguoidung (MaNguoiDung),
    INDEX idx_datphong_nhanvientao (MaNhanVienTao),
    INDEX idx_datphong_noiluutru (MaNoiLuuTru),
    INDEX idx_datphong_thoigian (NgayNhanPhong, NgayTraPhong),
    INDEX idx_datphong_trangthai (TrangThaiDatPhong)
) ENGINE=InnoDB;

-- 9. Chi tiết đặt phòng: PK ghép cho phép một đơn có nhiều phòng.
CREATE TABLE ChiTietDatPhong (
    MaDatPhong INT NOT NULL,
    MaPhong INT NOT NULL,
    GiaMoiDem DECIMAL(18,2) NOT NULL,
    TrangThaiNhanPhong VARCHAR(50) NULL,
    TrangThaiTraPhong VARCHAR(50) NULL,
    NgayCheckInThucTe DATETIME NULL,
    NgayCheckOutThucTe DATETIME NULL,
    PRIMARY KEY (MaDatPhong, MaPhong),
    CONSTRAINT FK_ChiTietDatPhong_DatPhong
        FOREIGN KEY (MaDatPhong) REFERENCES DatPhong(MaDatPhong)
        ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietDatPhong_Phong
        FOREIGN KEY (MaPhong) REFERENCES Phong(MaPhong),
    CONSTRAINT CK_ChiTietDatPhong_Gia CHECK (GiaMoiDem >= 0),
    CONSTRAINT CK_ChiTietDatPhong_CheckOut
        CHECK (NgayCheckOutThucTe IS NULL OR NgayCheckInThucTe IS NULL
               OR NgayCheckOutThucTe >= NgayCheckInThucTe),
    INDEX idx_chitiet_phong (MaPhong)
) ENGINE=InnoDB;

-- 10. Thanh toán: không UNIQUE(MaDatPhong), cho phép nhiều lần thanh toán/hoàn tiền.
CREATE TABLE ThanhToan (
    MaThanhToan INT AUTO_INCREMENT PRIMARY KEY,
    MaDatPhong INT NOT NULL,
    PhuongThucThanhToan VARCHAR(100) NOT NULL,
    SoTien DECIMAL(18,2) NOT NULL,
    TrangThaiThanhToan VARCHAR(50) NOT NULL,
    MaGiaoDich VARCHAR(150) NULL,
    NgayThanhToan DATETIME NULL,
    CONSTRAINT FK_ThanhToan_DatPhong
        FOREIGN KEY (MaDatPhong) REFERENCES DatPhong(MaDatPhong)
        ON DELETE CASCADE,
    CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien >= 0),
    INDEX idx_thanhtoan_trangthai (TrangThaiThanhToan),
    INDEX idx_thanhtoan_datphong (MaDatPhong)
) ENGINE=InnoDB;

-- 11. Đánh giá
CREATE TABLE DanhGia (
    MaDanhGia INT AUTO_INCREMENT PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaNoiLuuTru INT NOT NULL,
    DiemDanhGia TINYINT NOT NULL,
    NoiDung TEXT NULL,
    NgayTao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT UQ_NguoiDung_NoiLuuTru UNIQUE (MaNguoiDung, MaNoiLuuTru),
    CONSTRAINT FK_DanhGia_NguoiDung
        FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung),
    CONSTRAINT FK_DanhGia_NoiLuuTru
        FOREIGN KEY (MaNoiLuuTru) REFERENCES NoiLuuTru(MaNoiLuuTru)
        ON DELETE CASCADE,
    CONSTRAINT CK_DanhGia_Diem CHECK (DiemDanhGia BETWEEN 1 AND 5),
    INDEX idx_danhgia_noiluutru (MaNoiLuuTru),
    INDEX idx_danhgia_diem (DiemDanhGia)
) ENGINE=InnoDB;

-- =========================
-- DỮ LIỆU MẪU
-- =========================
-- 1. Bảng NguoiDung (10 dòng)

INSERT INTO NguoiDung (HoTen, Email, SoDT, MatKhauHash, TrangThaiHoatDong, VaiTro) VALUES
('Nguyễn Văn An', 'an.nguyen@gmail.com', '0901234567', 'hash_pass_1', 1, 'KhachHang'),
('Trần Thị Bích', 'bich.tran@gmail.com', '0912345678', 'hash_pass_2', 1, 'KhachHang'),
('Lê Hoàng Cường', 'cuong.le@gmail.com', '0923456789', 'hash_pass_3', 1, 'KhachHang'),
('Phạm Thị Dung', 'dung.pham@gmail.com', '0934567890', 'hash_pass_4', 1, 'KhachHang'),
('Hoàng Văn Em', 'em.hoang@gmail.com', '0945678901', 'hash_pass_5', 1, 'NhanVien'),
('Vũ Thị Phương', 'phuong.vu@gmail.com', '0956789012', 'hash_pass_6', 1, 'NhanVien'),
('Đặng Văn Giang', 'giang.dang@gmail.com', '0967890123', 'hash_pass_7', 1, 'KhachHang'),
('Bùi Thị Hương', 'huong.bui@gmail.com', '0978901234', 'hash_pass_8', 1, 'KhachHang'),
('Ngô Văn Hùng', 'hung.ngo@gmail.com', '0989012345', 'hash_pass_9', 1, 'QuanTriVien'),
('Dương Thị Linh', 'linh.duong@gmail.com', '0990123456', 'hash_pass_10', 1, 'KhachHang');

-- 2. Bảng NoiLuuTru (10 dòng)

INSERT INTO NoiLuuTru (TenNoiLuuTru, LoaiHinh, DiaChi, MoTa, TrangThaiHoatDong) VALUES
('Sunset Beach Resort', 'Resort', '123 Đường Trần Phú, Nha Trang, Khánh Hòa', 'Khu nghỉ dưỡng sát biển với view tuyệt đẹp.', 1),
('Green Valley Homestay', 'Homestay', '45 Nguyễn Chí Thanh, Đà Lạt, Lâm Đồng', 'Homestay phong cách mộc mạc giữa thung lũng thông.', 1),
('Grand Luxury Hotel', 'Khách sạn', '10 Lê Duẩn, Quận 1, TP. Hồ Chí Minh', 'Khách sạn 5 sao sang trọng giữa lòng thành phố.', 1),
('Cozy Corner Apartment', 'Căn hộ', '88 Phố Huế, Hoàn Kiếm, Hà Nội', 'Căn hộ ấm cúng đầy đủ tiện nghi cho gia đình.', 1),
('Seaview Villa Vũng Tàu', 'Villa', '12 Hạ Long, Phường 2, Vũng Tàu', 'Villa hồ bơi riêng nhìn thẳng ra biển Bãi Trước.', 1),
('Mountain Wind Homestay', 'Homestay', 'Thôn Bản Là, Sa Pa, Lào Cai', 'Trải nghiệm không gian văn hóa bản địa vùng cao.', 1),
('Riverside Boutique Hotel', 'Khách sạn', '56 Bạch Đằng, Hải Châu, Đà Nẵng', 'Khách sạn boutique cạnh sông Hàn thơ mộng.', 1),
('Pine Hill Cottage', 'Homestay', 'Đường Đống Đa, Đà Lạt, Lâm Đồng', 'Nhà gỗ nhỏ xinh dưới đồi thông.', 1),
('Central Park Hotel', 'Khách sạn', '20 Hùng Vương, Huế', 'Khách sạn gần kinh thành Huế cổ kính.', 1),
('Tropical Oasis Resort', 'Resort', 'Bãi Trường, Dương Tơ, Phú Quốc', 'Resort nhiệt đới lý tưởng cho kỳ nghỉ dưỡng.', 1);

-- 3. Bảng HinhAnhNoiLuuTru (10 dòng)

INSERT INTO HinhAnhNoiLuuTru (MaNoiLuuTru, DuongDanAnh, LaAnhDaiDien) VALUES
(1, '/images/sunset_1.jpg', 1),
(2, '/images/green_1.jpg', 1),
(3, '/images/grand_1.jpg', 1),
(4, '/images/cozy_1.jpg', 1),
(5, '/images/villa_1.jpg', 1),
(6, '/images/mountain_1.jpg', 1),
(7, '/images/riverside_1.jpg', 1),
(8, '/images/pine_1.jpg', 1),
(9, '/images/central_1.jpg', 1),
(10, '/images/tropical_1.jpg', 1);

-- 4. Bảng LoaiPhong (10 dòng)

INSERT INTO LoaiPhong (MaNoiLuuTru, TenLoaiPhong, MoTa, SucChua, GiaCoBan) VALUES
(1, 'Phòng Deluxe Hướng Biển', 'Phòng rộng 35m2 có ban công nhìn ra biển.', 2, 1200000.00),
(1, 'Phòng Suite Gia Đình', 'Phòng rộng 60m2, 2 giường lớn.', 4, 2500000.00),
(2, 'Phòng Đôi Tiêu Chuẩn', 'Phòng mộc mạc ấm cúng view đồi thông.', 2, 500000.00),
(3, 'Phòng Superior Twin', 'Phòng 2 giường đơn tiện nghi hiện đại.', 2, 1800000.00),
(3, 'Phòng Presidential Suite', 'Phòng Tổng thống siêu sang trọng tầng cao nhất.', 2, 5500000.00),
(4, 'Căn Hộ 1 Phòng Ngủ', 'Có sẵn bếp tự nấu ăn, máy giặt.', 2, 800000.00),
(5, 'Villa Nguyên Căn 3 Phòng Ngủ', 'Có hồ bơi riêng, BBQ sân vườn.', 8, 4500000.00),
(6, 'Phòng Tập Thể Dorm', 'Giường tầng sạch sẽ, giá sinh viên.', 1, 200000.00),
(7, 'Phòng Deluxe Sông Hàn', 'View trực diện cầu Rồng.', 2, 1500000.00),
(10, 'Bungalow Sát Biển', 'Bungalow gỗ phong cách nhiệt đới riêng tư.', 2, 2200000.00);

-- 5. Bảng Phong (10 dòng)

INSERT INTO Phong (MaLoaiPhong, SoPhong, TrangThaiPhong) VALUES
(1, '101', 'Trống'),
(1, '102', 'Đang ở'),
(2, '201', 'Trống'),
(3, '301', 'Trống'),
(4, '401', 'Đang ở'),
(5, '501', 'Trống'),
(7, 'V01', 'Trống'),
(8, 'D01', 'Trống'),
(9, '901', 'Đang ở'),
(10, 'B01', 'Trống');

-- 6. Bảng TienNghi (10 dòng)

INSERT INTO TienNghi (TenTienNghi, MoTa) VALUES
('Wi-Fi miễn phí', 'Internet tốc độ cao phủ sóng toàn khuôn viên'),
('Hồ bơi', 'Hồ bơi ngoài trời sạch sẽ'),
('Điều hòa không khí', 'Máy lạnh 2 chiều hiện đại'),
('Bãi đỗ xe', 'Bãi đỗ xe rộng rãi an toàn có bảo vệ'),
('Bữa sáng miễn phí', 'Buffet sáng đa dạng món ăn Á - Âu'),
('Dịch vụ phòng 24/7', 'Phục vụ đồ ăn thức uống tận phòng'),
('Bếp nấu ăn', 'Đầy đủ dụng cụ nấu nướng cơ bản'),
('Máy giặt', 'Máy giặt tự động hoặc chung'),
('Tivi màn hình phẳng', 'Tivi kết nối internet giải trí'),
('Ban công/Sân hiên', 'Không gian thư giãn ngoài trời');

-- 7. Bảng LoaiPhong_TienNghi (10 dòng liên kết)

INSERT INTO LoaiPhong_TienNghi (MaLoaiPhong, MaTienNghi) VALUES
(1, 1), (1, 3), (1, 5), (1, 9), (1, 10),
(2, 1), (2, 2), (2, 3), (2, 5), (2, 9);

-- 8. Bảng DatPhong (10 dòng)

INSERT INTO DatPhong (MaNguoiDung, MaNhanVienTao, MaNoiLuuTru, NgayNhanPhong, NgayTraPhong, SoLuongKhach, TongTien, TrangThaiDatPhong) VALUES
(1, 5, 1, '2026-06-10', '2026-06-13', 4, 7200000.00, 'Đã xác nhận'),
(2, NULL, 2, '2026-06-15', '2026-06-17', 2, 1000000.00, 'Đã xác nhận'),
(3, NULL, 3, '2026-07-01', '2026-07-04', 2, 5400000.00, 'Hoàn thành'),
(4, NULL, 4, '2026-07-05', '2026-07-07', 2, 1600000.00, 'Đã hủy'),
(7, NULL, 5, '2026-08-10', '2026-08-13', 6, 13500000.00, 'Đã xác nhận'),
(8, NULL, 6, '2026-08-15', '2026-08-16', 1, 200000.00, 'Hoàn thành'),
(10, NULL, 7, '2026-09-02', '2026-09-04', 2, 3000000.00, 'Đã xác nhận'),
(1, NULL, 2, '2026-09-10', '2026-09-12', 2, 1000000.00, 'Đã xác nhận'),
(2, NULL, 7, '2026-10-01', '2026-10-03', 2, 3000000.00, 'Hoàn thành'),
(3, NULL, 10, '2026-10-10', '2026-10-13', 2, 6600000.00, 'Đã xác nhận');

-- 9. Bảng ChiTietDatPhong (10 dòng)

INSERT INTO ChiTietDatPhong (MaDatPhong, MaPhong, GiaMoiDem, TrangThaiNhanPhong, TrangThaiTraPhong, NgayCheckInThucTe, NgayCheckOutThucTe) VALUES
(1, 1, 1200000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(1, 2, 1200000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(2, 3, 500000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(3, 4, 1800000.00, 'Đã nhận', 'Đã trả', '2026-07-01 14:00:00', '2026-07-04 11:30:00'),
(4, 5, 800000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(5, 7, 4500000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(6, 8, 200000.00, 'Đã nhận', 'Đã trả', '2026-08-15 15:00:00', '2026-08-16 10:00:00'),
(7, 9, 1500000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(8, 3, 500000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL),
(9, 9, 1500000.00, 'Đã nhận', 'Đã trả', '2026-10-01 14:00:00', '2026-10-03 12:00:00'),
(10, 10, 2200000.00, 'Chưa nhận', 'Chưa trả', NULL, NULL);

-- 10. Bảng ThanhToan (10 dòng)

INSERT INTO ThanhToan (MaDatPhong, PhuongThucThanhToan, SoTien, TrangThaiThanhToan, MaGiaoDich) VALUES
(1, 'Chuyển khoản ngân hàng', 7200000.00, 'Đã thanh toán', 'TXN001'),
(2, 'Thẻ tín dụng', 1000000.00, 'Đã thanh toán', 'TXN002'),
(3, 'Tiền mặt', 5400000.00, 'Đã thanh toán', 'TXN003'),
(4, 'Ví điện tử Momo', 1600000.00, 'Đã hoàn tiền', 'TXN004'),
(5, 'Chuyển khoản ngân hàng', 13500000.00, 'Đã thanh toán', 'TXN005'),
(6, 'Tiền mặt', 200000.00, 'Đã thanh toán', 'TXN006'),
(7, 'Thẻ tín dụng', 3000000.00, 'Chờ thanh toán', NULL),
(8, 'Ví điện tử ZaloPay', 1000000.00, 'Đã thanh toán', 'TXN008'),
(9, 'Chuyển khoản ngân hàng', 3000000.00, 'Đã thanh toán', 'TXN009'),
(10, 'Thẻ tín dụng', 6600000.00, 'Đã thanh toán', 'TXN010');

-- 11. Bảng DanhGia (10 dòng)

INSERT INTO DanhGia (MaNguoiDung, MaNoiLuuTru, DiemDanhGia, NoiDung) VALUES
(3, 3, 5, 'Khách sạn rất tuyệt vời, nhân viên thân thiện, phòng sạch sẽ.'),
(8, 6, 4, 'Homestay ấm cúng, view đẹp, anh chủ nhà rất nhiệt tình.'),
(2, 2, 5, 'Không gian yên bình, rất thích hợp để nghỉ ngơi cuối tuần.'),
(10, 7, 4, 'Vị trí trung tâm tiện đi lại, nhìn ra sông Hàn rất lãng mạn.'),
(1, 1, 5, 'Resort sát biển tuyệt đẹp, dịch vụ cao cấp đáng trải nghiệm.'),
(4, 4, 3, 'Căn hộ tạm ổn nhưng cách âm hơi kém chút.'),
(7, 5, 5, 'Villa quá đẹp và rộng rãi, hồ bơi sạch sẽ, gia đình rất thích.'),
(8, 8, 4, 'Nhà gỗ xinh xắn, không khí Đà Lạt rất chill.'),
(2, 9, 5, 'Khách sạn cổ kính, sạch sẽ, gần khu di tích.'),
(3, 10, 5, 'Bungalow sát biển tuyệt vời, hoàng hôn nhìn cực đẹp.');

-- Bật lại kiểm tra khóa ngoại


-- Ghi chú nghiệp vụ cần kiểm tra trong backend:
-- 1. Kiểm tra phòng bị trùng lịch trong transaction khi tạo/cập nhật đơn.
-- 2. Đảm bảo mọi phòng trong ChiTietDatPhong thuộc cùng MaNoiLuuTru
--    với DatPhong.MaNoiLuuTru (có thể kiểm tra bằng JOIN trong service).
-- 3. Kiểm tra VaiTro của MaNhanVienTao là 'NhanVien' hoặc 'QuanTriVien';
--    khóa ngoại chỉ bảo đảm người tạo tồn tại, không tự kiểm tra vai trò.
-- 4. Tổng tiền đơn phải được tính từ số phòng, số đêm và giá từng phòng.
