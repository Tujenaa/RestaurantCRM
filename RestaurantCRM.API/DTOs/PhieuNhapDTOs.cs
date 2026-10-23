using System.Collections.Generic;

namespace RestaurantCRM.API.DTOs
{
    public class PhieuNhapCreateRequest
    {
        public string? MaNhaCungCap { get; set; }
        public string? MaNhanVien { get; set; }
        public string? GhiChu { get; set; }
        public List<ChiTietPhieuNhapRequest> ChiTiet { get; set; } = new List<ChiTietPhieuNhapRequest>();
    }

    public class ChiTietPhieuNhapRequest
    {
        // Nhập hàng theo nguyên liệu
        public string? MaNguyenLieu { get; set; }

        // Hỗ trợ fallback nếu client cũ gửi MaMon
        public string? MaMon { get; set; }

        public string TargetMaNguyenLieu => !string.IsNullOrWhiteSpace(MaNguyenLieu) ? MaNguyenLieu : (MaMon ?? "");

        public decimal SoLuong { get; set; }
        public double DonGiaNhap { get; set; }
    }
}
