using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("DichVu")]
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        public int MaCoSo { get; set; }

        [Required]
        [StringLength(200)]
        public string TenDichVu { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public decimal? Gia { get; set; }

        [StringLength(50)]
        public string? DonViGia { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "hoat_dong";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // QUAN TRỌNG: Bỏ qua khi serialize JSON để tránh vòng lặp
        [JsonIgnore]
        [ForeignKey("MaCoSo")]
        public virtual CoSoKinhDoanh? CoSo { get; set; }
    }
}