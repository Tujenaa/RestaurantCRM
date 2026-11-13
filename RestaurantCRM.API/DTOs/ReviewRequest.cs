using System;
using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class DishReviewItemDto
    {
        public string MaMon { get; set; } = null!;
        /// <summary>NULL = khách không bấm sao riêng cho món (nếu có nhận xét thì lấy sao chung).</summary>
        public int? SoSao { get; set; }
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

    public class UpdateOrderReviewRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public string MaHoaDon { get; set; } = null!;
        public int SoSaoChung { get; set; }
        public string? NoiDungChung { get; set; }
        public List<DishReviewItemDto>? ChiTietMon { get; set; }
    }

    public class DishReviewDetailItemDto
    {
        public string MaChiTietDanhGia { get; set; } = null!;
        public string MaMon { get; set; } = null!;
        public string TenMon { get; set; } = null!;
        public string? DuongDanAnh { get; set; }
        /// <summary>NULL = món không được chấm riêng.</summary>
        public int? SoSao { get; set; }
        public string? NoiDung { get; set; }
    }

    public class OrderReviewDetailDto
    {
        public string MaDanhGia { get; set; } = null!;
        public string MaHoaDon { get; set; } = null!;
        public string MaKhachHang { get; set; } = null!;
        public string TenKhachHang { get; set; } = "Khách hàng";
        public int SoSaoChung { get; set; }
        public string? NoiDungChung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        public List<DishReviewDetailItemDto> ChiTietMon { get; set; } = new();
        public List<ReviewReplyDetailDto> HoiThoai { get; set; } = new();
    }

    public class ReviewReplyDetailDto
    {
        public string MaTraLoi { get; set; } = null!;
        public string? NoiDung { get; set; }
        public DateTime? NgayGui { get; set; }
        public string NguoiTraLoi { get; set; } = "Nhân viên Lẩu Phố";
    }

    public class ReplyRequest
    {
        public string NguoiGuiId { get; set; } = null!; // MaKhachHang hoặc MaNhanVien
        public string NoiDung { get; set; } = null!;
    }
}
