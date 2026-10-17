using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Data
{
    public class NexusDbContext : DbContext
    {
        public NexusDbContext(DbContextOptions<NexusDbContext> options)
            : base(options) { }

        // ============ DbSet ============
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<CoSoKinhDoanh> CoSoKinhDoanhs { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<UuDai> UuDais { get; set; }
        public DbSet<DiemThuong> DiemThuongs { get; set; }
        public DbSet<PhieuGiamGia> PhieuGiamGias { get; set; }
        public DbSet<BaoCao> BaoCaos { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }

        // ✅ BẢNG MỚI: Duyệt xác thực sinh viên
        public DbSet<YeuCauXacThuc> YeuCauXacThucs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============ UNIQUE CONSTRAINTS ============
            modelBuilder.Entity<NguoiDung>()
                .HasIndex(n => n.Email)
                .IsUnique();

            modelBuilder.Entity<PhieuGiamGia>()
                .HasIndex(p => p.MaCode)
                .IsUnique();

            // ============ DECIMAL PRECISION ============
            modelBuilder.Entity<CoSoKinhDoanh>(e =>
            {
                e.Property(c => c.ViDo).HasPrecision(10, 8);
                e.Property(c => c.KinhDo).HasPrecision(11, 8);
                e.Property(c => c.DanhGiaTrungBinh).HasPrecision(3, 2);
            });

            modelBuilder.Entity<DichVu>()
                .Property(d => d.Gia).HasPrecision(12, 0);

            modelBuilder.Entity<UuDai>()
                .Property(u => u.GiaTriGiam).HasPrecision(10, 2);

            // ============ QUAN HỆ ============

            // CoSoKinhDoanh - NguoiDung
            modelBuilder.Entity<CoSoKinhDoanh>()
                .HasOne(c => c.ChuSoHuu)
                .WithMany(n => n.DanhSachCoSo)
                .HasForeignKey(c => c.MaChuSoHuu)
                .OnDelete(DeleteBehavior.Restrict);

            // CoSoKinhDoanh - DanhMuc
            modelBuilder.Entity<CoSoKinhDoanh>()
                .HasOne(c => c.DanhMuc)
                .WithMany(d => d.DanhSachCoSo)
                .HasForeignKey(c => c.MaDanhMuc)
                .OnDelete(DeleteBehavior.SetNull);

            // DichVu - CoSoKinhDoanh
            modelBuilder.Entity<DichVu>()
                .HasOne(d => d.CoSo)
                .WithMany(c => c.DanhSachDichVu)
                .HasForeignKey(d => d.MaCoSo)
                .OnDelete(DeleteBehavior.Cascade);

            // DanhGia - CoSoKinhDoanh
            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.CoSo)
                .WithMany(c => c.DanhSachDanhGia)
                .HasForeignKey(d => d.MaCoSo)
                .OnDelete(DeleteBehavior.Cascade);

            // UuDai - CoSoKinhDoanh
            modelBuilder.Entity<UuDai>()
                .HasOne(u => u.CoSo)
                .WithMany(c => c.DanhSachUuDai)
                .HasForeignKey(u => u.MaCoSo)
                .OnDelete(DeleteBehavior.Cascade);

            // PhieuGiamGia - NguoiDung
            modelBuilder.Entity<PhieuGiamGia>()
                .HasOne(p => p.NguoiDung)
                .WithMany()
                .HasForeignKey(p => p.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // PhieuGiamGia - UuDai
            modelBuilder.Entity<PhieuGiamGia>()
                .HasOne(p => p.UuDai)
                .WithMany()
                .HasForeignKey(p => p.MaUuDai)
                .OnDelete(DeleteBehavior.Cascade);

            // DiemThuong - NguoiDung
            modelBuilder.Entity<DiemThuong>()
                .HasOne(d => d.NguoiDung)
                .WithMany()
                .HasForeignKey(d => d.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ BẢNG MỚI: YeuCauXacThuc - NguoiDung
            modelBuilder.Entity<YeuCauXacThuc>()
                .HasOne(y => y.NguoiDung)
                .WithMany()
                .HasForeignKey(y => y.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}