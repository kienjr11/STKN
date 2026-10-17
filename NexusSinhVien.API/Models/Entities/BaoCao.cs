using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("BaoCao")]
    public class BaoCao
    {
        [Key]
        public int MaBaoCao { get; set; }

        public int MaNguoiDung { get; set; }

        [StringLength(20)]
        public string LoaiDoiTuong { get; set; } = "co_so";

        public int MaDoiTuong { get; set; }

        public string? LyDo { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "cho_xu_ly";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [JsonIgnore]
        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}