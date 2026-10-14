using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NexusSinhVien.API.Models.Entities
{
    [Table("CoSoKinhDoanh")]
    public class CoSoKinhDoanh
    {
        [Key]
        public int MaCoSo { get; set; }

        public int MaChuSoHuu { get; set; }

        [Required]
        [StringLength(200)]
        public string TenCoSo { get; set; } = string.Empty;

        public int? MaDanhMuc { get; set; }

        public string? MoTa { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public decimal? ViDo { get; set; }
        public decimal? KinhDo { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? GioMoCua { get; set; }

        [StringLength(20)]
        public string TrangThai { get; set; } = "cho_duyet";

        public decimal DanhGiaTrungBinh { get; set; } = 0;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Chủ sở hữu - Ẩn để tránh vòng lặp với NguoiDung
        [JsonIgnore]
        [ForeignKey("MaChuSoHuu")]
        public virtual NguoiDung? ChuSoHuu { get; set; }

        // Danh mục - GIỮ LẠI vì frontend cần hiển thị
        [ForeignKey("MaDanhMuc")]
        public virtual DanhMuc? DanhMuc { get; set; }

        // GIỮ LẠI để hiển thị ở trang chi tiết
        public virtual ICollection<DichVu>? DanhSachDichVu { get; set; }

        // Ẩn để tránh vòng lặp
        [JsonIgnore]
        public virtual ICollection<DanhGia>? DanhSachDanhGia { get; set; }

        // GIỮ LẠI để hiển thị ưu đãi ở chi tiết
        public virtual ICollection<UuDai>? DanhSachUuDai { get; set; }
    }
}