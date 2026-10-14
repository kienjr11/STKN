using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("DanhMuc")]
    public class DanhMuc
    {
        [Key]
        public int MaDanhMuc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDanhMuc { get; set; } = string.Empty;

        [StringLength(10)]
        public string? BieuTuong { get; set; }

        public int? MaDanhMucCha { get; set; }

        public int ThuTuHienThi { get; set; } = 0;

        // QUAN TRỌNG: Bỏ qua khi serialize JSON để tránh vòng lặp
        [JsonIgnore]
        public virtual ICollection<CoSoKinhDoanh>? DanhSachCoSo { get; set; }
    }
}