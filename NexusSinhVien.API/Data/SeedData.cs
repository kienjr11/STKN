using Microsoft.EntityFrameworkCore;
using NexusSinhVien.API.Models.Entities;

namespace NexusSinhVien.API.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new NexusDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<NexusDbContext>>());

            // Bọc try-catch để debug
            try
            {
                // Kiểm tra và tạo admin
                if (!context.NguoiDungs.Any(n => n.Email == "admin@nexus.local"))
                {
                    context.NguoiDungs.Add(new NguoiDung
                    {
                        Email = "admin@nexus.local",
                        MatKhau = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        HoTen = "Quản trị viên",
                        LoaiNguoiDung = "quan_tri",
                        DaXacThuc = true,
                        DiemTichLuy = 0,
                        NgayTao = DateTime.Now,
                        NgayCapNhat = DateTime.Now
                    });
                    context.SaveChanges();
                    Console.WriteLine("✅ Đã tạo tài khoản admin");
                }
                else
                {
                    Console.WriteLine("ℹ️ Admin đã tồn tại, bỏ qua");
                }

                // Kiểm tra và tạo danh mục
                if (!context.DanhMucs.Any())
                {
                    context.DanhMucs.AddRange(
                        new DanhMuc { TenDanhMuc = "Ăn uống", BieuTuong = "🍜", ThuTuHienThi = 1 },
                        new DanhMuc { TenDanhMuc = "Nhà trọ", BieuTuong = "🏠", ThuTuHienThi = 2 },
                        new DanhMuc { TenDanhMuc = "Học tập", BieuTuong = "📚", ThuTuHienThi = 3 },
                        new DanhMuc { TenDanhMuc = "Tiện ích", BieuTuong = "🛒", ThuTuHienThi = 4 },
                        new DanhMuc { TenDanhMuc = "Việc làm thêm", BieuTuong = "💼", ThuTuHienThi = 5 },
                        new DanhMuc { TenDanhMuc = "Giải trí", BieuTuong = "🎮", ThuTuHienThi = 6 }
                    );
                    context.SaveChanges();
                    Console.WriteLine("✅ Đã tạo 6 danh mục");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI SeedData: {ex.Message}");
                Console.WriteLine($"Chi tiết: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}