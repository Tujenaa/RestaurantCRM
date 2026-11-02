using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class ReviewRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string? MaMon { get; set; }
        public string? MaHoaDon { get; set; }
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
    }

    public class DishReviewItemDto
    {
        public string MaMon { get; set; } = null!;
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
    }

    public class OrderReviewBatchRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string MaHoaDon { get; set; } = null!;
        public int SoSaoChung { get; set; }
        public string? NoiDungChung { get; set; }
        public List<DishReviewItemDto>? ChiTietMon { get; set; }
    }

    public class ReplyRequest
    {
        public string NguoiGuiId { get; set; } = null!; // MaKhachHang hoặc MaNhanVien
        public string NoiDung { get; set; } = null!;
    }
}
