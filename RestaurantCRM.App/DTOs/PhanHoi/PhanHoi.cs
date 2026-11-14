using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <summary>Lần khách sửa đánh giá gần nhất; null = chưa từng sửa.</summary>
        public DateTime? NgayCapNhat { get; set; }
        /// <summary>true = Hiển thị trên website, false = đã ẩn.</summary>
        public bool HienThi { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public int TongSoTraLoi { get; set; }
        /// <summary>"Chưa xử lý" | "Đã xử lý".</summary>
        public string TrangThaiXuLy { get; set; }
        /// <summary>Trạng thái xử lý do admin chỉnh (gửi trả lời cũng tự đặt = true).</summary>
        public bool DaXuLy { get; set; }
        public List<TraLoiPhanHoiDto> HoiThoai { get; set; } = new List<TraLoiPhanHoiDto>();
        /// <summary>Đánh giá từng món của đơn (bảng CHI_TIET_DANH_GIA).</summary>
        public List<DanhGiaMonDto> ChiTietMon { get; set; } = new List<DanhGiaMonDto>();

        /// <summary>Số món khách chấm riêng ở mức thấp (≤ 2 sao). Món không đánh giá  (SoSao NULL) không tính.</summary>
        public int SoMonBiChe { get { return ChiTietMon == null ? 0 : ChiTietMon.Count(m => m.SoSao.HasValue && m.SoSao.Value <= 2); } }
        /// <summary>Cần chú ý: điểm chung 1–2 sao hoặc có món bị chấm thấp.</summary>
        public bool CanChuY { get { return (SoSao.HasValue && SoSao.Value > 0 && SoSao.Value <= 2) || SoMonBiChe > 0; } }

        public bool DaTraLoi { get { return TongSoTraLoi > 0 || (HoiThoai != null && HoiThoai.Count > 0); } }
        public bool LaDaXuLy { get { return DaXuLy; } }

        /// <summary>Khách đã sửa đánh giá.</summary>
        public bool DaChinhSua { get { return NgayCapNhat.HasValue; } }
        /// <summary>Lần trả lời (gửi hoặc sửa) gần nhất của nhà hàng.</summary>
        public DateTime? NgayTraLoiCuoi
        {
            get { return HoiThoai == null ? null : HoiThoai.Where(t => t.NgayGui.HasValue).Select(t => t.NgayGui).DefaultIfEmpty(null).Max(); }
        }
        /// <summary>Khách sửa đánh giá sau lần trả lời gần nhất của nhà hàng (so ngày, không suy từ DaXuLy).</summary>
        public bool SuaSauPhanHoi
        {
            get { var tl = NgayTraLoiCuoi; return NgayCapNhat.HasValue && tl.HasValue && NgayCapNhat.Value > tl.Value; }
        }
        /// <summary>Hoạt động mới nhất của khách: lần sửa, nếu chưa sửa thì lần gửi.</summary>
        public DateTime? NgayHoatDong { get { return NgayCapNhat ?? NgayDanhGia; } }

        /// <summary>
        /// Trạng thái xử lý suy ra từ dữ liệu: Đã xử lý / Cần xem lại (khách sửa sau phản hồi) / Chưa xử lý.
        /// Ẩn/hiện trên website là thông tin riêng.
        /// </summary>
        public string TrangThaiHienThi
        {
            get
            {
                // Gọn 3 trạng thái: "đã sửa" xem ở cột Ngày gửi (✎), "chưa trả lời" xem ở khung chi tiết / bộ lọc Loại
                if (LaDaXuLy) return "Đã xử lý";
                if (SuaSauPhanHoi) return "Cần xem lại";   // khách sửa sau khi nhà hàng trả lời
                return "Chưa xử lý";
            }
        }
        public string DoiTuong { get { return string.IsNullOrWhiteSpace(TenMon) ? (string.IsNullOrWhiteSpace(MaHoaDon) ? "Dịch vụ chung" : "Đơn " + MaHoaDon) : TenMon; } }
    }

    public class DanhGiaMonDto
    {
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        /// <summary>NULL = khách không đánh giá  món này.</summary>
        public int? SoSao { get; set; }
        public string NoiDung { get; set; }
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
        /// <summary>= SoLuotChamRieng (giữ tên cũ).</summary>
        public int SoLuotDanhGia { get; set; }
        /// <summary>Số đơn được đánh giá có món này.</summary>
        public int SoLuotTrongDon { get; set; }
        /// <summary>Số lượt khách chấm sao riêng cho món.</summary>
        public int SoLuotChamRieng { get; set; }
        /// <summary>Trung bình sao chỉ trên các lượt chấm riêng; null nếu chưa có.</summary>
        public double? DiemTrungBinh { get; set; }
        /// <summary>Trung bình sao chung của các đơn có món này (chỉ để tham khảo).</summary>
        public double? DiemTheoDon { get; set; }
        /// <summary>Đủ lượt chấm riêng để kết luận (≥ 3).</summary>
        public bool DuDuLieu { get; set; }
        public int SoSao1 { get; set; }
        public int SoSao2 { get; set; }
        public int SoSao3 { get; set; }
        public int SoSao4 { get; set; }
        public int SoSao5 { get; set; }
        public List<PhanTichNhanXetDto> NhanXet { get; set; } = new List<PhanTichNhanXetDto>();
    }

    public class PhanTichNhanXetDto
    {
        public string MaDanhGia { get; set; }
        /// <summary>Sao khách chấm riêng cho món (NULL = không chấm riêng).</summary>
        public int? SoSao { get; set; }
        /// <summary>Sao chung của cả đơn.</summary>
        public int? SoSaoDon { get; set; }
        /// <summary>true = món có sao riêng (khách bấm sao hoặc viết nhận xét cho món).</summary>
        public bool ChamRieng { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        /// <summary>false = đánh giá đã bị ẩn trên website (vẫn tính vào thống kê).</summary>
        public bool HienThi { get; set; } = true;
    }
}
