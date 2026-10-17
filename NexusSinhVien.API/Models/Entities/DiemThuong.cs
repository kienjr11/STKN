using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("DiemThuong")]
    public class DiemThuong
    {
        [Key]
        public int MaDiem { get; set; }

        public int MaNguoiDung { get; set; }

        public int SoDiem { get; set; }

        [StringLength(100)]
        public string? HanhDong { get; set; }

        public int? MaThamChieu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [JsonIgnore]
        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }
    }
}