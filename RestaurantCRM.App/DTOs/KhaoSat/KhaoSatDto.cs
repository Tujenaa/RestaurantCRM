using System;
using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.DTOs.KhaoSat
{
    /// <summary>Giá trị trạng thái / loại câu hỏi lưu trong CSDL (khớp CHECK của KHAO_SAT, CAU_HOI_KHAO_SAT).</summary>
    public static class KhaoSatConst
    {
        public const string Nhap = "Draft";
        public const string DangMo = "Open";
        public const string DaDong = "Closed";
        public const string TracNghiem = "Option";
        public const string TuLuan = "Text";

        public static string TenTrangThai(string trangThai)
        {
            switch ((trangThai ?? "").Trim().ToLowerInvariant())
            {
                case "open": case "active": return "Đang mở";
                case "closed": return "Đã đóng";
                case "draft": return "Nháp";
                default: return string.IsNullOrWhiteSpace(trangThai) ? "Nháp" : trangThai;
            }
        }

        public static bool LaTracNghiem(string loai)
        {
            var v = (loai ?? "").Trim().ToLowerInvariant();
            return v == "option" || v == "tracnghiem";
        }

        public static string TenLoai(string loai) { return LaTracNghiem(loai) ? "Trắc nghiệm" : "Tự luận"; }

        public static string TenDoiTuong(int soDoiTuong) { return soDoiTuong > 0 ? soDoiTuong.ToString("N0") + " khách" : "Tất cả khách"; }
    }

    /// <summary>Một dòng khảo sát (GET api/AdminKhaoSat).</summary>
    public class KhaoSatDto
    {
        public string MaKhaoSat { get; set; }
        public string TieuDe { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string MaNhanVien { get; set; }
        public string NguoiTao { get; set; }
        public int SoCauHoi { get; set; }
        public int SoNguoiTraLoi { get; set; }
        /// <summary>Số khách được mời; 0 = tất cả khách hàng.</summary>
        public int SoDoiTuong { get; set; }

        public string TrangThaiHienThi { get { return KhaoSatConst.TenTrangThai(TrangThai); } }
        public string DoiTuongHienThi { get { return KhaoSatConst.TenDoiTuong(SoDoiTuong); } }
        public bool DangMo { get { return TrangThaiHienThi == "Đang mở"; } }
        public bool DaDong { get { return TrangThaiHienThi == "Đã đóng"; } }
        public bool LaNhap { get { return TrangThaiHienThi == "Nháp"; } }
    }

    /// <summary>Chi tiết khảo sát kèm câu hỏi và tùy chọn (GET api/AdminKhaoSat/{id}).</summary>
    public class KhaoSatChiTietDto
    {
        public string MaKhaoSat { get; set; }
        public string TieuDe { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
        public string MaNhanVien { get; set; }
        public int SoNguoiTraLoi { get; set; }
        public int SoDoiTuong { get; set; }
        /// <summary>Bộ "Hồ sơ sở thích": không đổi trạng thái / xóa / đặt đối tượng; sửa được dù đã có phiếu.</summary>
        public bool LaSoThich { get; set; }
        public List<CauHoiDto> CauHoi { get; set; } = new List<CauHoiDto>();

        public string TrangThaiHienThi { get { return KhaoSatConst.TenTrangThai(TrangThai); } }
        public string DoiTuongHienThi { get { return KhaoSatConst.TenDoiTuong(SoDoiTuong); } }
    }

    /// <summary>Một khách hàng được mời tham gia khảo sát (GET api/AdminKhaoSat/{id}/DoiTuong).</summary>
    public class DoiTuongKhaoSatDto
    {
        public string MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public bool DaHoanThanh { get; set; }
    }

    /// <summary>PUT api/AdminKhaoSat/{id}/DoiTuong — danh sách rỗng = tất cả khách hàng.</summary>
    public class DoiTuongUpdateRequest
    {
        public List<string> MaKhachHangs { get; set; } = new List<string>();
    }

    public class CauHoiDto
    {
        public string MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; }
        /// <summary>"Option" (trắc nghiệm) | "Text" (tự luận).</summary>
        public string LoaiCauHoi { get; set; }
        /// <summary>Đã có khách trả lời câu này (không đổi loại / xóa được).</summary>
        public bool DaCoTraLoi { get; set; }
        /// <summary>Câu trắc nghiệm cho chọn nhiều đáp án.</summary>
        public bool ChonNhieu { get; set; }
        /// <summary>Câu "Món yêu thích" của hồ sơ sở thích (đáp án gắn món).</summary>
        public bool LaCauMonYeuThich { get; set; }
        public List<TuyChonDto> TuyChon { get; set; } = new List<TuyChonDto>();

        public bool LaTracNghiem { get { return KhaoSatConst.LaTracNghiem(LoaiCauHoi); } }
    }

    public class TuyChonDto
    {
        public string MaTuyChon { get; set; }
        public string NoiDungTuyChon { get; set; }
        public string MaMon { get; set; }
        public string TenMon { get; set; }
        public bool NgungBan { get; set; }
        public bool DaCoNguoiChon { get; set; }
    }

    // ── Thống kê (GET api/AdminKhaoSat/{id}/results) ──────────────────────────

    public class KhaoSatKetQuaDto
    {
        public string MaKhaoSat { get; set; }
        public string TieuDe { get; set; }
        public string TrangThai { get; set; }
        public int TongSoNguoiTraLoi { get; set; }
        public int SoDoiTuong { get; set; }
        public List<CauHoiKetQuaDto> ChiTiet { get; set; } = new List<CauHoiKetQuaDto>();
    }

    public class CauHoiKetQuaDto
    {
        public string MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; }
        /// <summary>"TracNghiem" | "TuLuan".</summary>
        public string LoaiCauHoi { get; set; }
        public bool ChonNhieu { get; set; }
        /// <summary>Ghi chú hiển thị (vd. "Câu chọn nhiều, tổng có thể vượt 100%.").</summary>
        public string GhiChu { get; set; }
        public int SoLuotTraLoi { get; set; }
        public List<TuyChonKetQuaDto> KetQuaTN { get; set; }
        public List<TraLoiTuLuanDto> TraLoiTuLuan { get; set; }

        public bool LaTracNghiem { get { return KhaoSatConst.LaTracNghiem(LoaiCauHoi); } }
    }

    public class TuyChonKetQuaDto
    {
        public string MaTuyChon { get; set; }
        public string NoiDungTuyChon { get; set; }
        public int SoNguoiChon { get; set; }
        /// <summary>Phần trăm (0..100) trên số phiếu đã trả lời câu hỏi.</summary>
        public double TyLe { get; set; }
    }

    public class TraLoiTuLuanDto
    {
        public string NoiDung { get; set; }
        public string TenKhachHang { get; set; }
        public DateTime? NgayTraLoi { get; set; }
    }

    // ── Dữ liệu gửi lên ──────────────────────────────────────────────────────

    public class KhaoSatCreateRequest
    {
        public string TieuDe { get; set; }
        public string MaNhanVien { get; set; }
        public List<CauHoiCreateRequest> CauHois { get; set; } = new List<CauHoiCreateRequest>();
    }

    public class CauHoiCreateRequest
    {
        public string NoiDungCauHoi { get; set; }
        public string LoaiCauHoi { get; set; }
        public List<string> TuyChons { get; set; } = new List<string>();
        public bool ChonNhieu { get; set; }
    }

    public class KhaoSatUpdateRequest
    {
        public string TieuDe { get; set; }
    }

    public class CauHoiUpsertRequest
    {
        public string NoiDung { get; set; }
        public string LoaiCauHoi { get; set; }
        public List<string> TuyChons { get; set; } = new List<string>();
        public bool ChonNhieu { get; set; }
        /// <summary>Câu "Món yêu thích" (chỉ hồ sơ sở thích): đáp án lấy từ MaMons.</summary>
        public bool LaCauMonYeuThich { get; set; }
        public List<string> MaMons { get; set; } = new List<string>();
    }

    public class TuyChonUpsertRequest
    {
        public string NoiDungTuyChon { get; set; }
        /// <summary>Món gắn với đáp án (chỉ câu "Món yêu thích").</summary>
        public string MaMon { get; set; }
    }

    // ── Hồ sơ sở thích ───────────────────────────────────────────────────────

    /// <summary>Đáp án mà một khách đã khai báo (GET api/AdminKhaoSat/SoThich/KhachHang).</summary>
    public class SoThichKhachHangDto
    {
        public string MaKhachHang { get; set; }
        public List<string> MaTuyChons { get; set; } = new List<string>();
        /// <summary>Các câu khách có trả lời (mẫu số % theo từng câu).</summary>
        public List<string> MaCauHois { get; set; } = new List<string>();
    }

    /// <summary>Sở thích của 1 khách (GET api/AdminKhachHang/{id}/SoThich).</summary>
    public class SoThichCuaKhachDto
    {
        public bool DaKhaiBao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public List<SoThichCauHoiDto> CauHoi { get; set; } = new List<SoThichCauHoiDto>();
    }

    public class SoThichCauHoiDto
    {
        public string MaCauHoi { get; set; }
        public string CauHoi { get; set; }
        public string Loai { get; set; }
        public bool LaDiUng { get; set; }
        public List<SoThichDapAnDto> DapAn { get; set; } = new List<SoThichDapAnDto>();
        public string NoiDung { get; set; }
    }

    public class SoThichDapAnDto
    {
        public string MaTuyChon { get; set; }
        public string NoiDung { get; set; }
        public string MaMon { get; set; }
        public bool NgungBan { get; set; }
    }
}