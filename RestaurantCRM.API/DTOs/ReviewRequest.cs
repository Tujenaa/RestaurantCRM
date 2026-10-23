namespace RestaurantCRM.API.DTOs
{
    public class ReviewRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string MaMon { get; set; } = null!;
        public string? MaHoaDon { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
    }

    public class ReplyRequest
    {
        public string NguoiGuiId { get; set; } = null!; // MaKhachHang hoặc MaNhanVien
        public string NoiDung { get; set; } = null!;
    }
}
