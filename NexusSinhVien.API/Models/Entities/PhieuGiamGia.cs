using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("PhieuGiamGia")]
    public class PhieuGiamGia
    {
        [Key]
        public int MaPhieu { get; set; }

        public int MaNguoiDung { get; set; }

        public int MaUuDai { get; set; }

        [Required]
        [StringLength(50)]
        public string MaCode { get; set; } = string.Empty;

        public int SoDiemDaDung { get; set; } = 0;

        [StringLength(20)]
        public string TrangThai { get; set; } = "chua_dung";

        public DateTime? NgayHetHan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Quan hệ
        [JsonIgnore]
        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        // Giữ lại - cần để hiển thị tên ưu đãi
        [ForeignKey("MaUuDai")]
        public virtual UuDai? UuDai { get; set; }
    }
}