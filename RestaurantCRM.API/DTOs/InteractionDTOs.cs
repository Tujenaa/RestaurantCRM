namespace RestaurantCRM.API.DTOs
{
    public class ReplyRequest
    {
        public string NguoiGuiId { get; set; } = null!; // MaKhachHang hoặc MaNhanVien
        public string NoiDung { get; set; } = null!;
    }

    public class PhanHoiCreateRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string NoiDung { get; set; } = null!;
        public int? DanhGia { get; set; } // Điểm đánh giá dịch vụ chung
    }

    public class PhanHoiStatusUpdateRequest
    {
        public string TrangThai { get; set; } = null!;
        public string? MaNhanVien { get; set; } // Nhân viên tiếp nhận xử lý
    }
}
