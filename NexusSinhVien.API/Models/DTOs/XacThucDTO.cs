namespace NexusSinhVien.API.Models.DTOs
{
    // ✅ ĐÃ CẬP NHẬT: Thêm LoaiNguoiDung
    public record DangKyRequest(
        string Email,
        string MatKhau,
        string HoTen,
        string? SoDienThoai,
        string? MaSinhVien,
        string? TruongDaiHoc,
        string? LoaiNguoiDung
    );

    // DTO cho đăng nhập
    public record DangNhapRequest(
        string Email,
        string MatKhau);

    // DTO cho phản hồi đăng nhập
    public record DangNhapResponse(
        string Token,
        int MaNguoiDung,
        string Email,
        string HoTen,
        string LoaiNguoiDung,
        bool DaXacThuc);
}