using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Staybook_sidePj.Models.Database;

namespace Staybook_sidePj.Data;

public partial class StaybookDbContext : DbContext
{
    public StaybookDbContext(DbContextOptions<StaybookDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chitietdatphong> Chitietdatphong { get; set; }

    public virtual DbSet<Danhgia> Danhgia { get; set; }

    public virtual DbSet<Datphong> Datphong { get; set; }

    public virtual DbSet<Hinhanhnoiluutru> Hinhanhnoiluutru { get; set; }

    public virtual DbSet<Loaiphong> Loaiphong { get; set; }

    public virtual DbSet<Nguoidung> Nguoidung { get; set; }

    public virtual DbSet<Noiluutru> Noiluutru { get; set; }

    public virtual DbSet<Phong> Phong { get; set; }

    public virtual DbSet<Thanhtoan> Thanhtoan { get; set; }

    public virtual DbSet<Tiennghi> Tiennghi { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chitietdatphong>(entity =>
        {
            entity.HasKey(e => new { e.MaDatPhong, e.MaPhong }).HasName("PRIMARY");

            entity.ToTable("chitietdatphong");

            entity.HasIndex(e => e.MaPhong, "idx_chitiet_phong");

            entity.Property(e => e.NgayCheckInThucTe).HasColumnType("datetime");
            entity.Property(e => e.NgayCheckOutThucTe).HasColumnType("datetime");
            entity.Property(e => e.TrangThaiNhanPhong).HasMaxLength(50);
            entity.Property(e => e.TrangThaiTraPhong).HasMaxLength(50);

            entity.HasOne(d => d.MaDatPhongNavigation).WithMany(p => p.Chitietdatphong)
                .HasForeignKey(d => d.MaDatPhong)
                .HasConstraintName("chitietdatphong_ibfk_1");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Chitietdatphong)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chitietdatphong_ibfk_2");
        });

        modelBuilder.Entity<Danhgia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PRIMARY");

            entity.ToTable("danhgia");

            entity.HasIndex(e => new { e.MaNguoiDung, e.MaNoiLuuTru }, "UQ_NguoiDung_NoiLuuTru").IsUnique();

            entity.HasIndex(e => e.DiemDanhGia, "idx_danhgia_diem");

            entity.HasIndex(e => e.MaNoiLuuTru, "idx_danhgia_noiluutru");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.NoiDung).HasColumnType("text");

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("danhgia_ibfk_1");

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .HasConstraintName("danhgia_ibfk_2");
        });

        modelBuilder.Entity<Datphong>(entity =>
        {
            entity.HasKey(e => e.MaDatPhong).HasName("PRIMARY");

            entity.ToTable("datphong");

            entity.HasIndex(e => e.MaNguoiDung, "idx_datphong_nguoidung");

            entity.HasIndex(e => e.MaNoiLuuTru, "idx_datphong_noiluutru");

            entity.HasIndex(e => new { e.NgayNhanPhong, e.NgayTraPhong }, "idx_datphong_thoigian");

            entity.HasIndex(e => e.TrangThaiDatPhong, "idx_datphong_trangthai");

            entity.Property(e => e.NgayNhanPhong).HasColumnType("date");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.NgayTraPhong).HasColumnType("date");
            entity.Property(e => e.TrangThaiDatPhong).HasMaxLength(50);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.Datphong)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("datphong_ibfk_1");

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Datphong)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("datphong_ibfk_2");
        });

        modelBuilder.Entity<Hinhanhnoiluutru>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PRIMARY");

            entity.ToTable("hinhanhnoiluutru");

            entity.HasIndex(e => e.MaNoiLuuTru, "idx_hinhanh_noiluutru");

            entity.Property(e => e.DuongDanAnh).HasMaxLength(500);
            entity.Property(e => e.LaAnhDaiDien).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Hinhanhnoiluutru)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .HasConstraintName("hinhanhnoiluutru_ibfk_1");
        });

        modelBuilder.Entity<Loaiphong>(entity =>
        {
            entity.HasKey(e => e.MaLoaiPhong).HasName("PRIMARY");

            entity.ToTable("loaiphong");

            entity.HasIndex(e => e.GiaCoBan, "idx_loaiphong_giaban");

            entity.HasIndex(e => e.MaNoiLuuTru, "idx_loaiphong_noiluutru");

            entity.HasIndex(e => e.SucChua, "idx_loaiphong_succhua");

            entity.Property(e => e.MoTa).HasColumnType("text");
            entity.Property(e => e.TenLoaiPhong).HasMaxLength(150);

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Loaiphong)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .HasConstraintName("loaiphong_ibfk_1");

            entity.HasMany(d => d.MaTienNghi).WithMany(p => p.MaLoaiPhong)
                .UsingEntity<Dictionary<string, object>>(
                    "LoaiphongTiennghi",
                    r => r.HasOne<Tiennghi>().WithMany()
                        .HasForeignKey("MaTienNghi")
                        .HasConstraintName("loaiphong_tiennghi_ibfk_2"),
                    l => l.HasOne<Loaiphong>().WithMany()
                        .HasForeignKey("MaLoaiPhong")
                        .HasConstraintName("loaiphong_tiennghi_ibfk_1"),
                    j =>
                    {
                        j.HasKey("MaLoaiPhong", "MaTienNghi").HasName("PRIMARY");
                        j.ToTable("loaiphong_tiennghi");
                        j.HasIndex(new[] { "MaTienNghi" }, "MaTienNghi");
                    });
        });

        modelBuilder.Entity<Nguoidung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PRIMARY");

            entity.ToTable("nguoidung");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.Email, "idx_nguoidung_email");

            entity.HasIndex(e => e.TrangThaiHoatDong, "idx_nguoidung_trangthai");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhauHash).HasMaxLength(255);
            entity.Property(e => e.SoDt)
                .HasMaxLength(20)
                .HasColumnName("SoDT");
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValueSql("'1'");
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<Noiluutru>(entity =>
        {
            entity.HasKey(e => e.MaNoiLuuTru).HasName("PRIMARY");

            entity.ToTable("noiluutru");

            entity.HasIndex(e => e.LoaiHinh, "idx_noiluutru_loaihinh");

            entity.HasIndex(e => e.TrangThaiHoatDong, "idx_noiluutru_trangthai");

            entity.Property(e => e.DiaChi).HasMaxLength(300);
            entity.Property(e => e.LoaiHinh).HasMaxLength(100);
            entity.Property(e => e.MoTa).HasColumnType("text");
            entity.Property(e => e.TenNoiLuuTru).HasMaxLength(200);
            entity.Property(e => e.TrangThaiHoatDong).HasDefaultValueSql("'1'");
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PRIMARY");

            entity.ToTable("phong");

            entity.HasIndex(e => e.MaLoaiPhong, "idx_phong_loaiphong");

            entity.HasIndex(e => e.TrangThaiPhong, "idx_phong_trangthai");

            entity.Property(e => e.SoPhong).HasMaxLength(50);
            entity.Property(e => e.TrangThaiPhong).HasMaxLength(50);

            entity.HasOne(d => d.MaLoaiPhongNavigation).WithMany(p => p.Phong)
                .HasForeignKey(d => d.MaLoaiPhong)
                .HasConstraintName("phong_ibfk_1");
        });

        modelBuilder.Entity<Thanhtoan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PRIMARY");

            entity.ToTable("thanhtoan");

            entity.HasIndex(e => e.MaDatPhong, "MaDatPhong").IsUnique();

            entity.HasIndex(e => e.TrangThaiThanhToan, "idx_thanhtoan_trangthai");

            entity.Property(e => e.MaGiaoDich).HasMaxLength(100);
            entity.Property(e => e.NgayThanhToan)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.TrangThaiThanhToan).HasMaxLength(50);

            entity.HasOne(d => d.MaDatPhongNavigation).WithOne(p => p.Thanhtoan)
                .HasForeignKey<Thanhtoan>(d => d.MaDatPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("thanhtoan_ibfk_1");
        });

        modelBuilder.Entity<Tiennghi>(entity =>
        {
            entity.HasKey(e => e.MaTienNghi).HasName("PRIMARY");

            entity.ToTable("tiennghi");

            entity.Property(e => e.MoTa).HasColumnType("text");
            entity.Property(e => e.TenTienNghi).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
