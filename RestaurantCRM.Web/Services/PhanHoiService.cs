using System.Net.Http.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class PhanHoiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PhanHoiService> _logger;

    public PhanHoiService(IHttpClientFactory httpClientFactory, ILogger<PhanHoiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách đánh giá của riêng khách hàng (lịch sử cá nhân), xếp mới -> cũ
    /// </summary>
    public async Task<IReadOnlyList<PhanHoiItemViewModel>> GetByKhachHangAsync(string? maKhachHang)
    {
        if (string.IsNullOrWhiteSpace(maKhachHang))
        {
            return Array.Empty<PhanHoiItemViewModel>();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/DanhGia/KhachHang/{Uri.EscapeDataString(maKhachHang)}");
            if (response.IsSuccessStatusCode)
            {
                var apiReviews = await response.Content.ReadFromJsonAsync<List<ApiDanhGiaDto>>();
                if (apiReviews != null)
                {
                    return apiReviews
                        .OrderByDescending(x => x.NgayDanhGia ?? DateTime.MinValue)
                        .Select(MapFromApi)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không thể tải đánh giá của khách hàng {MaKhachHang} từ API ({Message}).", maKhachHang, ex.Message);
        }

        return Array.Empty<PhanHoiItemViewModel>();
    }

    /// <summary>
    /// Lấy danh sách đánh giá sắp xếp theo thứ tự mới nhất đến cũ nhất.
    /// </summary>
    public async Task<IReadOnlyList<PhanHoiItemViewModel>> GetRecentAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/AdminDanhGia");
            if (response.IsSuccessStatusCode)
            {
                var apiReviews = await response.Content.ReadFromJsonAsync<List<ApiDanhGiaDto>>();
                if (apiReviews != null)
                {
                    return apiReviews
                        .OrderByDescending(x => x.NgayDanhGia ?? DateTime.MinValue)
                        .Select(MapFromApi)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không thể tải đánh giá từ API ({Message}).", ex.Message);
        }

        return Array.Empty<PhanHoiItemViewModel>();
    }

    /// <summary>
    /// Lấy danh sách đánh giá và phản hồi của riêng một món ăn (mới nhất đến cũ nhất)
    /// </summary>
    public async Task<IReadOnlyList<PhanHoiItemViewModel>> GetByMonAnAsync(string maMon)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/DanhGia/MonAn/{Uri.EscapeDataString(maMon)}");
            if (response.IsSuccessStatusCode)
            {
                var apiReviews = await response.Content.ReadFromJsonAsync<List<ApiDanhGiaDto>>();
                if (apiReviews != null)
                {
                    return apiReviews
                        .OrderByDescending(x => x.NgayDanhGia ?? DateTime.MinValue)
                        .Select(MapFromApi)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không thể tải đánh giá món {MaMon} từ API ({Message}).", maMon, ex.Message);
        }

        return Array.Empty<PhanHoiItemViewModel>();
    }

    /// <summary>
    /// Tính điểm sao trung bình và số lượng đánh giá của một món ăn
    /// </summary>
    public async Task<(double AverageRating, int ReviewCount)> GetDishRatingSummaryAsync(string maMon)
    {
        var reviews = await GetByMonAnAsync(maMon);
        if (reviews == null || reviews.Count == 0)
        {
            return (5.0, 0);
        }

        double avg = reviews.Average(r => r.DanhGia);
        return (Math.Round(avg, 1), reviews.Count);
    }

    /// <summary>
    /// Thêm đánh giá món ăn
    /// </summary>
    public async Task<bool> AddReviewAsync(string maMon, string tenMon, int rating, string noiDung, string? maDonHang = null, string? customerName = null, string? customerId = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                MaKhachHang = string.IsNullOrWhiteSpace(customerId) ? "KH01" : customerId,
                MaMon = maMon,
                MaHoaDon = maDonHang,
                SoSao = Math.Clamp(rating, 1, 5),
                NoiDung = noiDung
            };
            var response = await client.PostAsJsonAsync("/api/DanhGia", payload);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Đã lưu đánh giá {MaMon} lên API thành công.", maMon);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không thể gửi đánh giá lên API ({Message}).", ex.Message);
        }

        return false;
    }

    /// <summary>
    /// Thêm phản hồi trao đổi lại cho một đánh giá
    /// </summary>
    public async Task<bool> AddReplyAsync(string maDanhGia, string noiDung, string senderName = "Khách hàng", string? customerId = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                NguoiGuiId = string.IsNullOrWhiteSpace(customerId) ? "KH01" : customerId,
                NoiDung = noiDung
            };
            var response = await client.PostAsJsonAsync($"/api/DanhGia/{maDanhGia}/Reply", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Lỗi gọi API reply: {Message}", ex.Message);
            return false;
        }
    }

    private static PhanHoiItemViewModel MapFromApi(ApiDanhGiaDto dto)
    {
        return new PhanHoiItemViewModel
        {
            MaDanhGia = dto.MaDanhGia ?? "",
            MaMon = dto.MaMon ?? "",
            TenMon = dto.TenMon ?? "Món ngon Lẩu Phố",
            MaDonHang = dto.MaHoaDon,
            TenKhachHang = dto.TenKhachHang ?? "Khách hàng",
            DanhGia = dto.SoSao ?? 5,
            NoiDung = dto.NoiDung ?? "",
            NgayGui = dto.NgayDanhGia ?? DateTime.Now,
            TraLois = (dto.HoiThoai ?? new List<ApiTraLoiDto>())
                .OrderByDescending(t => t.NgayGui ?? DateTime.MinValue)
                .Select(t => new TraLoiDanhGiaViewModel
                {
                    MaTraLoi = t.MaTraLoi ?? "",
                    NguoiGui = t.NguoiGui ?? "NhanVien",
                    NguoiTraLoi = t.NguoiTraLoi ?? "Nhân viên Lẩu Phố",
                    NoiDung = t.NoiDung ?? "",
                    NgayGui = t.NgayGui ?? DateTime.Now
                }).ToList()
        };
    }

    private class ApiDanhGiaDto
    {
        public string? MaDanhGia { get; set; }
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? MaHoaDon { get; set; }
        public int? SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        public string? TenKhachHang { get; set; }
        public List<ApiTraLoiDto>? HoiThoai { get; set; }
    }

    private class ApiTraLoiDto
    {
        public string? MaTraLoi { get; set; }
        public string? NguoiGui { get; set; }
        public string? NoiDung { get; set; }
        public DateTime? NgayGui { get; set; }
        public string? NguoiTraLoi { get; set; }
    }
}
