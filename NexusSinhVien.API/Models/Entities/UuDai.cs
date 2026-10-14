using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("UuDai")]
    public class UuDai
    {
        [Key]
        public int MaUuDai { get; set; }

        public int MaCoSo { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        [StringLength(20)]
        public string LoaiGiamGia { get; set; } = "phan_tram";

        public decimal? GiaTriGiam { get; set; }

        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        public int SoLuong { get; set; } = 0;
        public int SoLuongDaDung { get; set; } = 0;

        [StringLength(20)]
        public string TrangThai { get; set; } = "hoat_dong";

        // QUAN TRỌNG: Bỏ qua khi serialize JSON để tránh vòng lặp
        [JsonIgnore]
        [ForeignKey("MaCoSo")]
        public virtual CoSoKinhDoanh? CoSo { get; set; }
    }
}