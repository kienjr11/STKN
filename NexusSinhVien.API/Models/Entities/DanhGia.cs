using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("DanhGia")]
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        public int MaNguoiDung { get; set; }

        public int MaCoSo { get; set; }

        [Range(1, 5)]
        public byte DiemDanhGia { get; set; }

        public string? BinhLuan { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        // GIỮ LẠI - frontend cần hiển thị tên người đánh giá
        [ForeignKey("MaNguoiDung")]
        public virtual NguoiDung? NguoiDung { get; set; }

        // QUAN TRỌNG: Bỏ qua để tránh vòng lặp
        [JsonIgnore]
        [ForeignKey("MaCoSo")]
        public virtual CoSoKinhDoanh? CoSo { get; set; }
    }
}