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
        return TinhTomTatDiem(await GetByMonAnAsync(maMon));
    }

    /// <summary>
    /// Điểm trung bình chỉ tính trên các lượt khách chấm sao riêng cho món; không có lượt nào → (0, 0).
    /// </summary>
    public static (double AverageRating, int ReviewCount) TinhTomTatDiem(IEnumerable<PhanHoiItemViewModel>? reviews)
    {
        var diem = (reviews ?? Enumerable.Empty<PhanHoiItemViewModel>())
            .Where(r => r.DanhGia is >= 1 and <= 5)
            .Select(r => r.DanhGia!.Value)
            .ToList();
        return diem.Count == 0 ? (0, 0) : (Math.Round(diem.Average(), 1), diem.Count);
    }

    /// <summary>
    /// Kiểm tra xem đơn hàng đã được đánh giá chưa
    /// </summary>
    public async Task<bool> IsOrderReviewedAsync(string? maHoaDon)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon))
        {
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/DanhGia/KiemTraDonHang/{Uri.EscapeDataString(maHoaDon)}");
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<ApiKiemTraDto>();
                return res?.daDanhGia ?? false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không thể kiểm tra trạng thái đánh giá của đơn #{MaHoaDon}: {Message}", maHoaDon, ex.Message);
        }

        return false;
    }

    /// <summary>
    /// Lấy chi tiết toàn bộ đánh giá của 1 đơn hàng (Master & Detail)
    /// </summary>
    public async Task<ChiTietDanhGiaDonHangViewModel?> GetOrderReviewDetailAsync(string maHoaDon)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon)) return null;

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/DanhGia/DonHang/{Uri.EscapeDataString(maHoaDon)}");
            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content.ReadFromJsonAsync<ApiOrderReviewDetailDto>();
                if (dto != null)
                {
                    return new ChiTietDanhGiaDonHangViewModel
                    {
                        MaDanhGia = dto.MaDanhGia ?? "",
                        MaHoaDon = dto.MaHoaDon ?? maHoaDon,
                        MaKhachHang = dto.MaKhachHang ?? "",
                        TenKhachHang = dto.TenKhachHang ?? "Khách hàng",
                        SoSaoChung = dto.SoSaoChung,
                        NoiDungChung = dto.NoiDungChung,
                        NgayDanhGia = dto.NgayDanhGia,
                        ChiTietMon = (dto.ChiTietMon ?? new()).Select(ct => new ChiTietMonDanhGiaItemViewModel
                        {
                            MaChiTietDanhGia = ct.MaChiTietDanhGia ?? "",
                            MaMon = ct.MaMon ?? "",
                            TenMon = ct.TenMon ?? "Món ngon Lẩu Phố",
                            DuongDanAnh = ct.DuongDanAnh,
                            SoSao = ct.SoSao,
                            NoiDung = ct.NoiDung
                        }).ToList(),
                        HoiThoai = (dto.HoiThoai ?? new()).Select(t => new TraLoiDanhGiaViewModel
                        {
                            MaTraLoi = t.MaTraLoi ?? "",
                            NoiDung = t.NoiDung ?? "",
                            NgayGui = t.NgayGui ?? DateTime.Now,
                            NguoiTraLoi = t.NguoiTraLoi ?? "Nhân viên Lẩu Phố"
                        }).ToList()
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi lấy chi tiết đánh giá đơn #{MaHoaDon} từ API", maHoaDon);
        }

        return null;
    }

    /// <summary>
    /// Gửi đánh giá trọn gói cho toàn bộ đơn hàng (chuẩn Master-Detail)
    /// </summary>
    public async Task<(bool Success, string Message)> SubmitOrderReviewBatchAsync(
        string customerId,
        string maHoaDon,
        int orderRating,
        string? orderComment,
        List<DishReviewInputItem>? dishReviews)
    {
        if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(maHoaDon))
        {
            return (false, "Thông tin đơn hàng hoặc khách hàng không hợp lệ.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                MaKhachHang = customerId.Trim(),
                MaHoaDon = maHoaDon.Trim(),
                SoSaoChung = Math.Clamp(orderRating, 1, 5),
                NoiDungChung = string.IsNullOrWhiteSpace(orderComment) ? null : orderComment.Trim(),
                ChiTietMon = MonChamRieng(dishReviews)
            };

            var response = await client.PostAsJsonAsync("/api/DanhGia/DonHang", payload);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Đánh giá đơn hàng thành công!");
            }

            var errContent = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errContent) ? "Không thể gửi đánh giá. Vui lòng thử lại sau." : errContent.Trim().Trim('"'));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi đánh giá trọn gói đơn hàng #{MaHoaDon}", maHoaDon);
            return (false, "Đã xảy ra lỗi kết nối khi gửi đánh giá. Vui lòng thử lại sau.");
        }
    }

    /// <summary>
    /// Cập nhật chỉnh sửa đánh giá trọn gói cho toàn bộ đơn hàng
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateOrderReviewBatchAsync(
        string customerId,
        string maHoaDon,
        int orderRating,
        string? orderComment,
        List<DishReviewInputItem>? dishReviews)
    {
        if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(maHoaDon))
        {
            return (false, "Thông tin đơn hàng hoặc khách hàng không hợp lệ.");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                MaKhachHang = customerId.Trim(),
                MaHoaDon = maHoaDon.Trim(),
                SoSaoChung = Math.Clamp(orderRating, 1, 5),
                NoiDungChung = string.IsNullOrWhiteSpace(orderComment) ? null : orderComment.Trim(),
                ChiTietMon = MonChamRieng(dishReviews)
            };

            var response = await client.PutAsJsonAsync("/api/DanhGia/DonHang", payload);
            if (response.IsSuccessStatusCode)
            {
                return (true, "Cập nhật đánh giá thành công!");
            }

            var errContent = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errContent) ? "Không thể cập nhật đánh giá. Vui lòng thử lại sau." : errContent.Trim().Trim('"'));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật đánh giá đơn hàng #{MaHoaDon}", maHoaDon);
            return (false, "Đã xảy ra lỗi kết nối khi cập nhật đánh giá. Vui lòng thử lại sau.");
        }
    }

    /// <summary>
    /// Chỉ gửi món khách chấm riêng (bấm sao) hoặc có nhận xét. Món chỉ có nhận xét gửi SoSao = null
    /// để API lấy sao chung; món không gửi → API lưu NULL (không đánh giá riêng).
    /// </summary>
    private static List<object> MonChamRieng(List<DishReviewInputItem>? dishReviews)
    {
        return (dishReviews ?? new List<DishReviewInputItem>())
            .Where(d => !string.IsNullOrWhiteSpace(d.MaMon) && (d.ChamRieng || !string.IsNullOrWhiteSpace(d.NoiDung)))
            .Select(d => (object)new
            {
                MaMon = d.MaMon,
                SoSao = d.ChamRieng ? Math.Clamp(d.DanhGia, 1, 5) : (int?)null,
                NoiDung = string.IsNullOrWhiteSpace(d.NoiDung) ? null : d.NoiDung.Trim()
            })
            .ToList();
    }

    /// <summary>
    /// Thêm phản hồi trao đổi lại cho một đánh giá
    /// </summary>
    public async Task<bool> AddReplyAsync(string maDanhGia, string noiDung, string senderName = "Khách hàng", string? customerId = null)
    {
        if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(maDanhGia))
        {
            return false;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                NguoiGuiId = customerId.Trim(),
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
            TenMon = dto.TenMon ?? (string.IsNullOrEmpty(dto.MaMon) ? "Đánh giá chung đơn hàng" : "Món ngon Lẩu Phố"),
            MaDonHang = dto.MaHoaDon,
            TenKhachHang = dto.TenKhachHang ?? "Khách hàng",
            DanhGia = dto.SoSao,
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

    private class ApiKiemTraDto
    {
        public string? maHoaDon { get; set; }
        public bool daDanhGia { get; set; }
    }

    private class ApiOrderReviewDetailDto
    {
        public string? MaDanhGia { get; set; }
        public string? MaHoaDon { get; set; }
        public string? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public int SoSaoChung { get; set; }
        public string? NoiDungChung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
        public List<ApiDishReviewDetailDto>? ChiTietMon { get; set; }
        public List<ApiTraLoiDto>? HoiThoai { get; set; }
    }

    private class ApiDishReviewDetailDto
    {
        public string? MaChiTietDanhGia { get; set; }
        public string? MaMon { get; set; }
        public string? TenMon { get; set; }
        public string? DuongDanAnh { get; set; }
        public int? SoSao { get; set; }
        public string? NoiDung { get; set; }
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
