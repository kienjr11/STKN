using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Data
{
    public class NexusDbContext : DbContext
    {
        public NexusDbContext(DbContextOptions<NexusDbContext> options)
            : base(options) { }

        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<CoSoKinhDoanh> CoSoKinhDoanhs { get; set; }
        public DbSet<DichVu> DichVus { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<UuDai> UuDais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NguoiDung>()
                .HasIndex(n => n.Email).IsUnique();

            modelBuilder.Entity<CoSoKinhDoanh>()
                .HasOne(c => c.ChuSoHuu)
                .WithMany(n => n.DanhSachCoSo)
                .HasForeignKey(c => c.MaChuSoHuu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DichVu>()
                .HasOne(d => d.CoSo)
                .WithMany(c => c.DanhSachDichVu)
                .HasForeignKey(d => d.MaCoSo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DanhGia>()
                .HasOne(d => d.CoSo)
                .WithMany(c => c.DanhSachDanhGia)
                .HasForeignKey(d => d.MaCoSo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UuDai>()
                .HasOne(u => u.CoSo)
                .WithMany(c => c.DanhSachUuDai)
                .HasForeignKey(u => u.MaCoSo)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}