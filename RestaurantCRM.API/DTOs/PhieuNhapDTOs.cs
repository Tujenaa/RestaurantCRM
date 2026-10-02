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
        public string MaMon { get; set; } = null!;
        public int SoLuong { get; set; }
        public double DonGiaNhap { get; set; }
    }
}
