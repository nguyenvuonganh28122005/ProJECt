using Microsoft.EntityFrameworkCore;
using StayBook.API.Models;

namespace StayBook.API.Data
{
    public class StayBookDbContext : DbContext
    {
        public StayBookDbContext(
            DbContextOptions<StayBookDbContext> options)
            : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
        public DbSet<NoiLuuTru> NoiLuuTrus => Set<NoiLuuTru>();
        public DbSet<HinhAnhNoiLuuTru> HinhAnhNoiLuuTrus => Set<HinhAnhNoiLuuTru>();
        public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
        public DbSet<Phong> Phongs => Set<Phong>();
        public DbSet<TienNghi> TienNghis => Set<TienNghi>();
        public DbSet<LoaiPhongTienNghi> LoaiPhongTienNghis => Set<LoaiPhongTienNghi>();
        public DbSet<DatPhong> DatPhongs => Set<DatPhong>();
        public DbSet<ChiTietDatPhong> ChiTietDatPhongs => Set<ChiTietDatPhong>();
        public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
        public DbSet<DanhGia> DanhGias => Set<DanhGia>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Email).IsUnique();
                entity.Property(x => x.HoTen).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
                entity.Property(x => x.SoDT).HasMaxLength(20).IsRequired();
                entity.Property(x => x.MatKhauHash).HasMaxLength(500).IsRequired();
                entity.Property(x => x.VaiTro).HasMaxLength(30).IsRequired();
            });

            modelBuilder.Entity<NoiLuuTru>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TenNoiLuuTru).HasMaxLength(200).IsRequired();
                entity.Property(x => x.LoaiHinh).HasMaxLength(50).IsRequired();
                entity.Property(x => x.DiaChi).HasMaxLength(500).IsRequired();
                entity.Property(x => x.MoTa).HasMaxLength(4000);
            });

            modelBuilder.Entity<HinhAnhNoiLuuTru>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.DuongDanAnh).HasMaxLength(2048).IsRequired();
                entity.HasOne(x => x.NoiLuuTru)
                    .WithMany(x => x.HinhAnhs)
                    .HasForeignKey(x => x.MaNoiLuuTru)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LoaiPhong>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TenLoaiPhong).HasMaxLength(120).IsRequired();
                entity.Property(x => x.MoTa).HasMaxLength(2000);
                entity.Property(x => x.GiaCoBan).HasPrecision(18, 2);
                entity.HasOne(x => x.NoiLuuTru)
                    .WithMany(x => x.LoaiPhongs)
                    .HasForeignKey(x => x.MaNoiLuuTru)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Phong>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.SoPhong).HasMaxLength(30).IsRequired();
                entity.Property(x => x.TrangThaiPhong).HasMaxLength(30).IsRequired();
                entity.HasIndex(x => new { x.MaLoaiPhong, x.SoPhong }).IsUnique();
                entity.HasOne(x => x.LoaiPhong)
                    .WithMany(x => x.Phongs)
                    .HasForeignKey(x => x.MaLoaiPhong)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TienNghi>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TenTienNghi).HasMaxLength(100).IsRequired();
                entity.Property(x => x.MoTa).HasMaxLength(1000);
            });

            modelBuilder.Entity<LoaiPhongTienNghi>(entity =>
            {
                entity.HasKey(x => new { x.MaLoaiPhong, x.MaTienNghi });
                entity.HasOne(x => x.LoaiPhong)
                    .WithMany(x => x.TienNghis)
                    .HasForeignKey(x => x.MaLoaiPhong)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.TienNghi)
                    .WithMany(x => x.LoaiPhongs)
                    .HasForeignKey(x => x.MaTienNghi)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DatPhong>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TongTien).HasPrecision(18, 2);
                entity.Property(x => x.TrangThaiDatPhong).HasMaxLength(30).IsRequired();
                entity.Property(x => x.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint("CK_DatPhong_Ngay", "[NgayTraPhong] > [NgayNhanPhong]");
                    table.HasCheckConstraint("CK_DatPhong_SoLuongKhach", "[SoLuongKhach] > 0");
                    table.HasCheckConstraint("CK_DatPhong_TongTien", "[TongTien] >= 0");
                });
                entity.HasOne(x => x.NguoiDung)
                    .WithMany(x => x.DatPhongs)
                    .HasForeignKey(x => x.MaNguoiDung)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChiTietDatPhong>(entity =>
            {
                entity.HasKey(x => new { x.MaDatPhong, x.MaPhong });
                entity.Property(x => x.GiaMoiDem).HasPrecision(18, 2);
                entity.Property(x => x.TrangThaiNhanPhong).HasMaxLength(30).IsRequired();
                entity.Property(x => x.TrangThaiTraPhong).HasMaxLength(30).IsRequired();
                entity.HasOne(x => x.DatPhong)
                    .WithMany(x => x.ChiTietDatPhongs)
                    .HasForeignKey(x => x.MaDatPhong)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Phong)
                    .WithMany(x => x.ChiTietDatPhongs)
                    .HasForeignKey(x => x.MaPhong)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ThanhToan>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.MaDatPhong).IsUnique();
                entity.Property(x => x.PhuongThucThanhToan).HasMaxLength(50).IsRequired();
                entity.Property(x => x.SoTien).HasPrecision(18, 2);
                entity.Property(x => x.TrangThaiThanhToan).HasMaxLength(30).IsRequired();
                entity.Property(x => x.MaGiaoDich).HasMaxLength(100);
                entity.HasOne(x => x.DatPhong)
                    .WithOne(x => x.ThanhToan)
                    .HasForeignKey<ThanhToan>(x => x.MaDatPhong)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DanhGia>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.NoiDung).HasMaxLength(2000);
                entity.Property(x => x.NgayTao).HasDefaultValueSql("GETDATE()");
                entity.ToTable(table => table.HasCheckConstraint("CK_DanhGia_Diem", "[DiemDanhGia] BETWEEN 1 AND 5"));
                entity.HasOne(x => x.NguoiDung)
                    .WithMany(x => x.DanhGias)
                    .HasForeignKey(x => x.MaNguoiDung)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.NoiLuuTru)
                    .WithMany(x => x.DanhGias)
                    .HasForeignKey(x => x.MaNoiLuuTru)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}