using System;
using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.DTOs.PhanHoi
{
    /// <summary>
    /// Phản hồi / góp ý của khách hàng (GET api/AdminDanhGia).
    /// CSDL không còn bảng PHAN_HOI: phản hồi là DANH_GIA, câu trả lời của nhân viên là TRA_LOI_DANH_GIA.
    /// </summary>
    public class PhanHoiDto
    {
        public string MaDanhGia { get; set; }
        public string MaKhachHang { get; set; }
        public string MaHoaDon { get; set; }
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public int? SoSao { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        /// <summary>true = đang hiển thị trên website, false = đã ẩn.</summary>
        public bool HienThi { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public int TongSoTraLoi { get; set; }
        /// <summary>"Chưa trả lời" | "Đã trả lời" | "Đã ẩn".</summary>
        public string TrangThaiXuLy { get; set; }
        public List<TraLoiPhanHoiDto> HoiThoai { get; set; } = new List<TraLoiPhanHoiDto>();

        public bool DaTraLoi { get { return TongSoTraLoi > 0 || (HoiThoai != null && HoiThoai.Count > 0); } }
        public string TrangThaiHienThi
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TrangThaiXuLy)) return TrangThaiXuLy;
                return !HienThi ? "Đã ẩn" : DaTraLoi ? "Đã trả lời" : "Chưa trả lời";
            }
        }
        public string DoiTuong { get { return string.IsNullOrWhiteSpace(TenMon) ? (string.IsNullOrWhiteSpace(MaHoaDon) ? "Dịch vụ chung" : "Đơn " + MaHoaDon) : TenMon; } }
    }

    public class TraLoiPhanHoiDto
    {
        public string MaTraLoi { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayGui { get; set; }
        public string MaNhanVien { get; set; }
        public string NguoiTraLoi { get; set; }
    }

    public class PhanHoiReplyRequest
    {
        public string NguoiGuiId { get; set; }
        public string NoiDung { get; set; }
    }

    public class PhanHoiTrangThaiRequest
    {
        public bool HienThi { get; set; }
    }
}