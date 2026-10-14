using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MatKhau { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(20)]
        public string LoaiNguoiDung { get; set; } = "sinh_vien";

        [StringLength(20)]
        public string? MaSinhVien { get; set; }

        [StringLength(100)]
        public string? TruongDaiHoc { get; set; }

        [StringLength(255)]
        public string? AnhDaiDien { get; set; }

        public bool DaXacThuc { get; set; } = false;

        public int DiemTichLuy { get; set; } = 0;

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // QUAN TRỌNG: Bỏ qua khi serialize JSON để tránh vòng lặp
        [JsonIgnore]
        public virtual ICollection<CoSoKinhDoanh>? DanhSachCoSo { get; set; }

        [JsonIgnore]
        public virtual ICollection<DanhGia>? DanhSachDanhGia { get; set; }
    }
}