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
            entity.HasKey(e => new { e.MaDatPhong, e.MaPhong }).HasName("PK_ChiTietDatPhong");

            entity.ToTable("ChiTietDatPhong");

            entity.HasIndex(e => e.MaPhong).HasDatabaseName("IX_ChiTietDatPhong_MaPhong");

            entity.Property(e => e.NgayCheckInThucTe).HasColumnType("datetime2(0)");
            entity.Property(e => e.NgayCheckOutThucTe).HasColumnType("datetime2(0)");
            entity.Property(e => e.TrangThaiNhanPhong).HasMaxLength(50);
            entity.Property(e => e.TrangThaiTraPhong).HasMaxLength(50);

            entity.HasOne(d => d.MaDatPhongNavigation).WithMany(p => p.Chitietdatphong)
                .HasForeignKey(d => d.MaDatPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ChiTietDatPhong_DatPhong");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Chitietdatphong)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ChiTietDatPhong_Phong");
        });

        modelBuilder.Entity<Danhgia>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK_DanhGia");

            entity.ToTable("DanhGia");

            entity.HasIndex(e => new { e.MaNguoiDung, e.MaNoiLuuTru }).IsUnique().HasDatabaseName("UQ_DanhGia_NguoiDung_NoiLuuTru");

            entity.HasIndex(e => e.MaNoiLuuTru).HasDatabaseName("IX_DanhGia_MaNoiLuuTru");

            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("SYSDATETIME()")
                .HasColumnType("datetime2(0)");
            entity.Property(e => e.NoiDung).HasMaxLength(1000);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhGia_NguoiDung");

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DanhGia_NoiLuuTru");
        });

        modelBuilder.Entity<Datphong>(entity =>
        {
            entity.HasKey(e => e.MaDatPhong).HasName("PK_DatPhong");

            entity.ToTable("DatPhong");

            entity.HasIndex(e => e.MaNguoiDung).HasDatabaseName("IX_DatPhong_MaNguoiDung");

            entity.HasIndex(e => new { e.MaNoiLuuTru, e.NgayNhanPhong, e.NgayTraPhong, e.TrangThaiDatPhong })
                .HasDatabaseName("IX_DatPhong_MaNoiLuuTru_Ngay");
            entity.HasIndex(e => e.MaNhanVienTao).HasDatabaseName("IX_DatPhong_MaNhanVienTao");

            entity.Property(e => e.NgayNhanPhong).HasColumnType("date");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("SYSDATETIME()")
                .HasColumnType("datetime2(0)");
            entity.Property(e => e.NgayTraPhong).HasColumnType("date");
            entity.Property(e => e.TrangThaiDatPhong).HasMaxLength(50);

            entity.HasOne(d => d.MaNguoiDungNavigation).WithMany(p => p.Datphong)
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_NguoiDung");

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Datphong)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_NoiLuuTru");

            entity.HasOne<Nguoidung>()
                .WithMany()
                .HasForeignKey(d => d.MaNhanVienTao)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_DatPhong_NhanVienTao");
        });

        modelBuilder.Entity<Hinhanhnoiluutru>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK_HinhAnhNoiLuuTru");

            entity.ToTable("HinhAnhNoiLuuTru");

            entity.HasIndex(e => e.MaNoiLuuTru).HasDatabaseName("IX_HinhAnhNoiLuuTru_MaNoiLuuTru");

            entity.Property(e => e.DuongDanAnh).HasMaxLength(500);
            entity.Property(e => e.LaAnhDaiDien).HasDefaultValue(false);

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Hinhanhnoiluutru)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_HinhAnhNoiLuuTru_NoiLuuTru");
        });

        modelBuilder.Entity<Loaiphong>(entity =>
        {
            entity.HasKey(e => e.MaLoaiPhong).HasName("PK_LoaiPhong");

            entity.ToTable("LoaiPhong");

            entity.HasIndex(e => e.MaNoiLuuTru).HasDatabaseName("IX_LoaiPhong_MaNoiLuuTru");
            entity.HasIndex(e => new { e.GiaCoBan, e.SucChua }).HasDatabaseName("IX_LoaiPhong_GiaCoBan_SucChua");

            entity.Property(e => e.MoTa).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TenLoaiPhong).HasMaxLength(150);

            entity.HasOne(d => d.MaNoiLuuTruNavigation).WithMany(p => p.Loaiphong)
                .HasForeignKey(d => d.MaNoiLuuTru)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_LoaiPhong_NoiLuuTru");

            entity.HasMany(d => d.MaTienNghi).WithMany(p => p.MaLoaiPhong)
                .UsingEntity<Dictionary<string, object>>(
                    "LoaiPhong_TienNghi",
                    r => r.HasOne<Tiennghi>().WithMany()
                        .HasForeignKey("MaTienNghi")
                        .OnDelete(DeleteBehavior.NoAction)
                        .HasConstraintName("FK_LPTN_TienNghi"),
                    l => l.HasOne<Loaiphong>().WithMany()
                        .HasForeignKey("MaLoaiPhong")
                        .OnDelete(DeleteBehavior.NoAction)
                        .HasConstraintName("FK_LPTN_LoaiPhong"),
                    j =>
                    {
                        j.HasKey("MaLoaiPhong", "MaTienNghi").HasName("PK_LoaiPhong_TienNghi");
                        j.ToTable("LoaiPhong_TienNghi");
                        j.HasIndex(new[] { "MaTienNghi" }).HasDatabaseName("IX_LPTN_MaTienNghi");
                    });
        });

        modelBuilder.Entity<Nguoidung>(entity =>
        {
            entity.HasKey(e => e.MaNguoiDung).HasName("PK_NguoiDung");

            entity.ToTable("NguoiDung");

            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_NguoiDung_Email");
            entity.HasIndex(e => e.NormalizedUserName).IsUnique().HasDatabaseName("UX_NguoiDung_NormalizedUserName");
            entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("IX_NguoiDung_NormalizedEmail");
            entity.HasIndex(e => new { e.VaiTro, e.TrangThaiHoatDong }).HasDatabaseName("IX_NguoiDung_VaiTro_TrangThai");

            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.SecurityStamp).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ConcurrencyStamp).HasColumnType("nvarchar(max)");
            entity.Property(e => e.SoDt).HasColumnType("nvarchar(max)").HasColumnName("SoDT");
            entity.Property(e => e.VaiTro).HasMaxLength(30);
            entity.Property(e => e.NgayTao).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSDATETIME()");
        });

        modelBuilder.Entity<Noiluutru>(entity =>
        {
            entity.HasKey(e => e.MaNoiLuuTru).HasName("PK_NoiLuuTru");

            entity.ToTable("NoiLuuTru");

            entity.Property(e => e.MoTa).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DiaChi).HasMaxLength(300);
            entity.Property(e => e.LoaiHinh).HasMaxLength(50);
            entity.Property(e => e.TenNoiLuuTru).HasMaxLength(200);
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK_Phong");

            entity.ToTable("Phong");

            entity.HasIndex(e => new { e.MaLoaiPhong, e.TrangThaiPhong }).HasDatabaseName("IX_Phong_MaLoaiPhong_TrangThai");

            entity.Property(e => e.SoPhong).HasMaxLength(30);
            entity.Property(e => e.TrangThaiPhong).HasMaxLength(50);

            entity.HasOne(d => d.MaLoaiPhongNavigation).WithMany(p => p.Phong)
                .HasForeignKey(d => d.MaLoaiPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Phong_LoaiPhong");
        });

        modelBuilder.Entity<Thanhtoan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK_ThanhToan");

            entity.ToTable("ThanhToan");

            entity.HasIndex(e => e.MaDatPhong).HasDatabaseName("IX_ThanhToan_MaDatPhong");
            entity.HasIndex(e => e.TrangThaiThanhToan).HasDatabaseName("IX_ThanhToan_TrangThai");

            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(100);
            entity.Property(e => e.MaGiaoDich).HasMaxLength(150);
            entity.Property(e => e.NgayThanhToan)
                .HasColumnType("datetime2(0)");
            entity.Property(e => e.TrangThaiThanhToan).HasMaxLength(50);

            entity.HasOne(d => d.MaDatPhongNavigation).WithMany(p => p.Thanhtoan)
                .HasForeignKey(d => d.MaDatPhong)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_ThanhToan_DatPhong");
        });

        modelBuilder.Entity<Tiennghi>(entity =>
        {
            entity.HasKey(e => e.MaTienNghi).HasName("PK_TienNghi");

            entity.ToTable("TienNghi");

            entity.Property(e => e.MoTa).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TenTienNghi).HasMaxLength(150);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
