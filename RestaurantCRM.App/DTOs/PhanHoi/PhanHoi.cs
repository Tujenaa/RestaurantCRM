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
        /// <summary>"Chưa xử lý" | "Đã xử lý".</summary>
        public string TrangThaiXuLy { get; set; }
        /// <summary>Trạng thái xử lý do admin chỉnh (gửi trả lời cũng tự đặt = true).</summary>
        public bool DaXuLy { get; set; }
        public List<TraLoiPhanHoiDto> HoiThoai { get; set; } = new List<TraLoiPhanHoiDto>();

        public bool DaTraLoi { get { return TongSoTraLoi > 0 || (HoiThoai != null && HoiThoai.Count > 0); } }
        public bool LaDaXuLy { get { return DaXuLy; } }
        /// <summary>Chỉ còn 2 trạng thái xử lý; ẩn/hiện trên website là thông tin riêng.</summary>
        public string TrangThaiHienThi { get { return LaDaXuLy ? "Đã xử lý" : "Chưa xử lý"; } }
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

    public class PhanTichDanhGiaMonDto
    {
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public int SoLuotDanhGia { get; set; }
        public double? DiemTrungBinh { get; set; }
        public int SoSao1 { get; set; }
        public int SoSao2 { get; set; }
        public int SoSao3 { get; set; }
        public int SoSao4 { get; set; }
        public int SoSao5 { get; set; }
        public List<PhanTichNhanXetDto> NhanXet { get; set; } = new List<PhanTichNhanXetDto>();
    }

    public class PhanTichNhanXetDto
    {
        public int? SoSao { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
    }
}
