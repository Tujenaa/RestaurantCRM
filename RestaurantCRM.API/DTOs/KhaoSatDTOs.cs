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
        public string LoaiCauHoi { get; set; } = null!;   // "TracNghiem" | "TuLuan"
        public List<string> TuyChons { get; set; } = new List<string>();
    }

    // ── Thêm / Sửa câu hỏi độc lập ───────────────────────────────────────────
    public class CauHoiUpsertRequest
    {
        public string NoiDung { get; set; } = null!;
        public string LoaiCauHoi { get; set; } = null!;   // "TracNghiem" | "TuLuan"
        /// <summary>Danh sách nội dung tùy chọn (chỉ áp dụng với TracNghiem).</summary>
        public List<string> TuyChons { get; set; } = new List<string>();
    }

    // ── Thêm / Sửa tùy chọn câu hỏi ─────────────────────────────────────────
    public class TuyChonUpsertRequest
    {
        public string NoiDungTuyChon { get; set; } = null!;
    }
}

