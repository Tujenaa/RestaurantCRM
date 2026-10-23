using System.Net.Http.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class PhanHoiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PhanHoiService> _logger;

    // Bộ nhớ in-memory fallback khởi tạo sẵn dữ liệu phong phú (gồm cả phản hồi xếp mới -> cũ)
    private readonly List<PhanHoiItemViewModel> _items = new()
    {
        new()
        {
            MaDanhGia = "DG001",
            MaMon = "M01",
            TenMon = "Lẩu Thái chua cay",
            DuongDanAnh = "https://images.unsplash.com/photo-1547928576-a4a33237cbc3?auto=format&fit=crop&w=800&q=80",
            MaDonHang = "DH00198",
            TenKhachHang = "Nguyễn Văn Hùng",
            DanhGia = 5,
            NoiDung = "Nước lẩu Thái rất đậm đà, vị chua cay hài hòa, hải sản kèm theo tươi giòn. Giao hàng đúng giờ!",
            NgayGui = DateTime.Now.AddDays(-1).AddHours(-3),
            TraLois = new List<TraLoiDanhGiaViewModel>
            {
                new()
                {
                    MaTraLoi = "TL002",
                    NguoiGui = "KhachHang",
                    NguoiTraLoi = "Nguyễn Văn Hùng",
                    NoiDung = "Dạ lần sau mình sẽ gọi thêm phần thịt bò Mỹ nữa nhé quán!",
                    NgayGui = DateTime.Now.AddHours(-12)
                },
                new()
                {
                    MaTraLoi = "TL001",
                    NguoiGui = "NhanVien",
                    NguoiTraLoi = "Quản lý Lẩu Phố",
                    NoiDung = "Lẩu Phố cảm ơn anh Hùng rất nhiều ạ! Chúc anh có những bữa ăn ngon miệng cùng gia đình.",
                    NgayGui = DateTime.Now.AddDays(-1).AddHours(-1)
                }
            }
        },
        new()
        {
            MaDanhGia = "DG002",
            MaMon = "M05",
            TenMon = "Bò Mỹ thái lát",
            DuongDanAnh = "https://images.unsplash.com/photo-1558030006-450675393462?auto=format&fit=crop&w=800&q=80",
            MaDonHang = "DH00231",
            TenKhachHang = "Trần Thị Mai",
            DanhGia = 4,
            NoiDung = "Thịt bò mềm tan, vân mỡ đều đẹp. Đóng khay giữ lạnh cẩn thận.",
            NgayGui = DateTime.Now.AddDays(-2).AddHours(-5),
            TraLois = new List<TraLoiDanhGiaViewModel>
            {
                new()
                {
                    MaTraLoi = "TL003",
                    NguoiGui = "NhanVien",
                    NguoiTraLoi = "Chăm sóc khách hàng Lẩu Phố",
                    NoiDung = "Lẩu Phố chân thành cảm ơn chị Mai. Nhà hàng luôn cam kết nhập thịt bò chuẩn chứng nhận mỗi ngày ạ.",
                    NgayGui = DateTime.Now.AddDays(-2).AddHours(-3)
                }
            }
        },
        new()
        {
            MaDanhGia = "DG003",
            MaMon = "M03",
            TenMon = "Lẩu nấm thanh đạm",
            DuongDanAnh = "https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=800&q=80",
            MaDonHang = "DH00198",
            TenKhachHang = "Lê Hoàng Long",
            DanhGia = 5,
            NoiDung = "Nước dùng thanh ngọt tự nhiên không gắt bột ngọt, các loại nấm tươi sạch ăn rất nhẹ bụng.",
            NgayGui = DateTime.Now.AddDays(-4),
            TraLois = new List<TraLoiDanhGiaViewModel>()
        },
        new()
        {
            MaDanhGia = "DG004",
            MaMon = "M06",
            TenMon = "Tôm sú tươi",
            DuongDanAnh = "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?auto=format&fit=crop&w=800&q=80",
            MaDonHang = "DH00198",
            TenKhachHang = "Phạm Quỳnh Chi",
            DanhGia = 4,
            NoiDung = "Tôm sú tươi giòn, nhúng lẩu vị ngọt lịm. Shipper giao rất thân thiện.",
            NgayGui = DateTime.Now.AddDays(-6),
            TraLois = new List<TraLoiDanhGiaViewModel>()
        }
    };

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
                if (apiReviews != null && apiReviews.Count > 0)
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
            _logger.LogWarning("Không thể tải đánh giá của khách hàng {MaKhachHang} từ API ({Message}), chuyển sang fallback.", maKhachHang, ex.Message);
        }

        lock (_items)
        {
            return _items
                .Where(x => string.Equals(x.MaKhachHang, maKhachHang, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.NgayGui)
                .Select(x =>
                {
                    x.TraLois = x.TraLois.OrderByDescending(t => t.NgayGui).ToList();
                    return x;
                })
                .ToList();
        }
    }

    /// <summary>
    /// Lấy danh sách đánh giá sắp xếp theo thứ tự mới nhất đến cũ nhất.
    /// Các câu trả lời trong từng đánh giá cũng được sắp xếp từ mới nhất đến cũ nhất.
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
                if (apiReviews != null && apiReviews.Count > 0)
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
            _logger.LogWarning("Không thể tải danh giá từ API ({Message}), chuyển sang fallback.", ex.Message);
        }

        lock (_items)
        {
            return _items
                .OrderByDescending(x => x.NgayGui)
                .Select(x =>
                {
                    // Đảm bảo TraLois sắp xếp mới nhất đến cũ nhất
                    x.TraLois = x.TraLois.OrderByDescending(t => t.NgayGui).ToList();
                    return x;
                })
                .ToList();
        }
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
                if (apiReviews != null && apiReviews.Count > 0)
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
            _logger.LogWarning("Không thể tải đánh giá món {MaMon} từ API ({Message}), dùng dữ liệu local.", maMon, ex.Message);
        }

        lock (_items)
        {
            return _items
                .Where(x => string.Equals(x.MaMon, maMon, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.NgayGui)
                .Select(x =>
                {
                    x.TraLois = x.TraLois.OrderByDescending(t => t.NgayGui).ToList();
                    return x;
                })
                .ToList();
        }
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
        var newReview = new PhanHoiItemViewModel
        {
            MaDanhGia = "DG" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            MaMon = maMon,
            TenMon = tenMon,
            MaDonHang = maDonHang,
            MaKhachHang = customerId ?? "KH01",
            TenKhachHang = string.IsNullOrWhiteSpace(customerName) ? "Khách hàng thân thiết" : customerName,
            DanhGia = Math.Clamp(rating, 1, 5),
            NoiDung = noiDung,
            NgayGui = DateTime.Now,
            TraLois = new List<TraLoiDanhGiaViewModel>()
        };

        // Đồng bộ lên API nếu có thể
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                MaKhachHang = string.IsNullOrWhiteSpace(customerId) ? "KH01" : customerId,
                MaMon = maMon,
                MaHoaDon = maDonHang,
                SoSao = rating,
                NoiDung = noiDung
            };
            var response = await client.PostAsJsonAsync("/api/DanhGia", payload);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Đã lưu đánh giá {MaMon} lên API thành công.", maMon);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Không thể gửi đánh giá lên API ({Message}), lưu vào cache cục bộ.", ex.Message);
        }

        // Luôn lưu vào danh sách cục bộ để hiển thị ngay lập tức
        lock (_items)
        {
            _items.Insert(0, newReview);
        }

        return true;
    }

    /// <summary>
    /// Thêm phản hồi trao đổi lại cho một đánh giá
    /// </summary>
    public async Task<bool> AddReplyAsync(string maDanhGia, string noiDung, string senderName = "Khách hàng", string? customerId = null)
    {
        var newReply = new TraLoiDanhGiaViewModel
        {
            MaTraLoi = "TL" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
            NguoiGui = "KhachHang",
            NguoiTraLoi = senderName,
            NoiDung = noiDung,
            NgayGui = DateTime.Now
        };

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                NguoiGuiId = string.IsNullOrWhiteSpace(customerId) ? "KH01" : customerId,
                NoiDung = noiDung
            };
            await client.PostAsJsonAsync($"/api/DanhGia/{maDanhGia}/Reply", payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Lỗi gọi API reply: {Message}", ex.Message);
        }

        lock (_items)
        {
            var review = _items.FirstOrDefault(x => x.MaDanhGia == maDanhGia);
            if (review != null)
            {
                // Thêm vào đầu để mới nhất hiển thị trước
                review.TraLois.Insert(0, newReply);
            }
        }

        return true;
    }

    private static PhanHoiItemViewModel MapFromApi(ApiDanhGiaDto dto)
    {
        return new PhanHoiItemViewModel
        {
            MaDanhGia = dto.MaDanhGia ?? "",
            MaMon = dto.MaMon ?? "",
            TenMon = dto.TenMon ?? "Món ngon Lẩu Phố",
            TenKhachHang = dto.TenKhachHang ?? "Khách hàng",
            DanhGia = dto.SoSao ?? 5,
            NoiDung = dto.NoiDung ?? "",
            NgayGui = dto.NgayDanhGia ?? DateTime.Now,
            TraLois = (dto.HoiThoai ?? new List<ApiTraLoiDto>())
                .OrderByDescending(t => t.NgayGui ?? DateTime.MinValue) // Sắp xếp phản hồi mới -> cũ
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
