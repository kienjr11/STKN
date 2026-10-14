namespace NexusSinhVien.API.Models.DTOs
{
    // DTO cho đăng ký
    public record DangKyRequest(
        string Email,
        string MatKhau,
        string HoTen,
        string? SoDienThoai,
        string? MaSinhVien,
        string? TruongDaiHoc);

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