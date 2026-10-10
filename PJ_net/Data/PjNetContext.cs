using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PJ_net.Models;

namespace PJ_net.Data;

public partial class PjNetContext : DbContext
{
    public PjNetContext(DbContextOptions<PjNetContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<LoThuoc> LoThuocs { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhomThuoc> NhomThuocs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<Thuoc> Thuocs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => new { e.MaHd, e.MaLo }).HasName("PK__ChiTietH__5557FA952B37FE87");

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHoa__MaHD__6383C8BA");

            entity.HasOne(d => d.MaLoNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaLo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietHoa__MaLo__6477ECF3");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HoaDon__2725A6E01B455A12");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.AnhDonThuoc)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.GhiChuKhachHang).HasMaxLength(255);
            entity.Property(e => e.HinhThuc).HasMaxLength(20);
            entity.Property(e => e.LoaiDonTuVan).HasMaxLength(20);
            entity.Property(e => e.MaDonThuocDienTu)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.NgayLap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TongTien)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("ChoXacNhan");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__HoaDon__MaKH__5BE2A6F2");
        });

        modelBuilder.Entity<LoThuoc>(entity =>
        {
            entity.HasKey(e => e.MaLo).HasName("PK__LoThuoc__2725C7563E91F033");

            entity.ToTable("LoThuoc");

            entity.Property(e => e.GiaNhap).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaNcc).HasColumnName("MaNCC");
            entity.Property(e => e.NgaySx).HasColumnName("NgaySX");
            entity.Property(e => e.SoLo)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaNccNavigation).WithMany(p => p.LoThuocs)
                .HasForeignKey(d => d.MaNcc)
                .HasConstraintName("FK__LoThuoc__MaNCC__5812160E");

            entity.HasOne(d => d.MaThuocNavigation).WithMany(p => p.LoThuocs)
                .HasForeignKey(d => d.MaThuoc)
                .HasConstraintName("FK__LoThuoc__MaThuoc__571DF1D5");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNcc).HasName("PK__NhaCungC__3A185DEB19744A34");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.MaNcc).HasColumnName("MaNCC");
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TenNcc)
                .HasMaxLength(150)
                .HasColumnName("TenNCC");
        });

        modelBuilder.Entity<NhomThuoc>(entity =>
        {
            entity.HasKey(e => e.MaNhom).HasName("PK__NhomThuo__234F91CD2218D8EE");

            entity.ToTable("NhomThuoc");

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenNhom).HasMaxLength(100);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk).HasName("PK__TaiKhoan__27250070B32BEA2A");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0A6444319").IsUnique();

            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VaiTro).HasDefaultValue(1);
        });

        modelBuilder.Entity<Thuoc>(entity =>
        {
            entity.HasKey(e => e.MaThuoc).HasName("PK__Thuoc__4BB1F6209C63DC77");

            entity.ToTable("Thuoc");

            entity.Property(e => e.AnhThuoc)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.DangBaoChe).HasMaxLength(50);
            entity.Property(e => e.DonViTinh).HasMaxLength(20);
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.HoatChat).HasMaxLength(150);
            entity.Property(e => e.KeDon).HasDefaultValue(false);
            entity.Property(e => e.TenThuoc).HasMaxLength(150);

            entity.HasOne(d => d.MaNhomNavigation).WithMany(p => p.Thuocs)
                .HasForeignKey(d => d.MaNhom)
                .HasConstraintName("FK__Thuoc__MaNhom__52593CB8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
