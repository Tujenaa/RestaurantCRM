namespace RestaurantCRM.API.DTOs
{
    public class KhaoSatCreateRequest
    {
        public string TieuDe { get; set; } = null!;
        public string? MaNhanVien { get; set; }
        public List<CauHoiCreateRequest> CauHois { get; set; } = new List<CauHoiCreateRequest>();
    }

    public class CauHoiCreateRequest
    {
        public string NoiDungCauHoi { get; set; } = null!;
        public string LoaiCauHoi { get; set; } = null!;
        public List<string> TuyChons { get; set; } = new List<string>();
    }
}
