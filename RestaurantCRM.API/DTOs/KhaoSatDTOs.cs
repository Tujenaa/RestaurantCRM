namespace RestaurantCRM.API.DTOs
{
    // ── Tạo khảo sát ──────────────────────────────────────────────────────────
    public class KhaoSatCreateRequest
    {
        public string TieuDe { get; set; } = null!;
        public string? MaNhanVien { get; set; }
        public List<CauHoiCreateRequest> CauHois { get; set; } = new List<CauHoiCreateRequest>();
    }

    // ── Cập nhật tiêu đề khảo sát ─────────────────────────────────────────────
    public class KhaoSatUpdateRequest
    {
        public string TieuDe { get; set; } = null!;
    }

    // ── Tạo câu hỏi (dùng trong POST tạo khảo sát) ───────────────────────────
    public class CauHoiCreateRequest
    {
        public string NoiDungCauHoi { get; set; } = null!;
        public string LoaiCauHoi { get; set; } = null!;   // "Option" (trắc nghiệm) | "Text" (tự luận); vẫn nhận "TracNghiem" | "TuLuan"
        public List<string> TuyChons { get; set; } = new List<string>();
        /// <summary>Câu trắc nghiệm cho chọn nhiều đáp án.</summary>
        public bool ChonNhieu { get; set; }
    }

    // ── Thêm / Sửa câu hỏi độc lập ───────────────────────────────────────────
    public class CauHoiUpsertRequest
    {
        public string NoiDung { get; set; } = null!;
        public string LoaiCauHoi { get; set; } = null!;   // "Option" (trắc nghiệm) | "Text" (tự luận); vẫn nhận "TracNghiem" | "TuLuan"
        /// <summary>Danh sách nội dung tùy chọn (chỉ áp dụng với TracNghiem, không dùng cho câu món yêu thích).</summary>
        public List<string> TuyChons { get; set; } = new List<string>();
        /// <summary>Câu trắc nghiệm cho chọn nhiều đáp án.</summary>
        public bool ChonNhieu { get; set; }
        /// <summary>Câu "Món yêu thích" của hồ sơ sở thích: đáp án lấy từ <see cref="MaMons"/>, tự đặt Option + chọn nhiều.</summary>
        public bool LaCauMonYeuThich { get; set; }
        /// <summary>Mã các món làm đáp án (chỉ dùng khi <see cref="LaCauMonYeuThich"/> = true).</summary>
        public List<string> MaMons { get; set; } = new List<string>();
    }

    // ── Cập nhật đối tượng khảo sát (rỗng = tất cả khách hàng) ──────────────
    public class DoiTuongUpdateRequest
    {
        public List<string> MaKhachHangs { get; set; } = new List<string>();
    }

    // ── Thêm / Sửa tùy chọn câu hỏi ─────────────────────────────────────────
    public class TuyChonUpsertRequest
    {
        /// <summary>Nội dung đáp án. Với đáp án gắn món, để trống thì lấy tên món.</summary>
        public string? NoiDungTuyChon { get; set; }
        /// <summary>Món gắn với đáp án (chỉ chấp nhận ở câu "Món yêu thích" của hồ sơ sở thích).</summary>
        public string? MaMon { get; set; }
    }

    // ── Hồ sơ sở thích (phía khách) ─────────────────────────────────────────
    public class SoThichLuuRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public List<SoThichCauTraLoi> CauTraLoi { get; set; } = new List<SoThichCauTraLoi>();
    }

    public class SoThichCauTraLoi
    {
        public string MaCauHoi { get; set; } = null!;
        public List<string> MaTuyChons { get; set; } = new List<string>();
        public string? NoiDung { get; set; }
    }

    public class MonYeuThichRequest
    {
        public string MaKhachHang { get; set; } = null!;
        public bool YeuThich { get; set; }
    }
}
