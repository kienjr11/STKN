using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("YeuCauXacThuc")]
    public class YeuCauXacThuc
    {
        [Key]
        public int MaYeuCau { get; set; }

        public int MaNguoiDung { get; set; }

        [Required]
        [StringLength(20)]
        public string MaSinhVien { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TruongDaiHoc { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AnhTheSinhVien { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AnhCCCD { get; set; } = string.Empty;

        public string? GhiChu { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "cho_duyet";
        // "cho_duyet", "da_duyet", "tu_choi"

        public string? LyDoTuChoi { get; set; }

        public int? MaNguoiDuyet { get; set; }

        public DateTime? NgayDuyet { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Quan hệ
        [JsonIgnore]
        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}