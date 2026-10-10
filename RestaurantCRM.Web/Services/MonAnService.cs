using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class MonAnService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MonAnService> _logger;

    // Built-in fallback catalog matching database seeds
    private static readonly List<MonAnViewModel> FallbackMenu = new()
    {
        new() { MaMon = "M01", MaLoaiMon = "LM01", TenMon = "Lẩu Thái chua cay", LoaiMon = "Nước lẩu", MoTa = "Nước lẩu Thái chua cay, thơm sả và lá chanh tươi mát, chuẩn vị truyền thống.", DuongDanAnh = "/images/default.jpg", DonGia = 180000, SoLuong = 37, TrangThai = "InStock", BieuTuong = "🥘", Tone = "red", Tag = "Bán chạy" },
        new() { MaMon = "M02", MaLoaiMon = "LM01", TenMon = "Lẩu bò nhúng dấm", LoaiMon = "Nước lẩu", MoTa = "Nước lẩu bò nhúng dấm chua dịu thanh tao, thơm nồng tỏi phi giòn rụm.", DuongDanAnh = "/images/default.jpg", DonGia = 220000, SoLuong = 31, TrangThai = "InStock", BieuTuong = "🍲", Tone = "gold", Tag = "Đặc sắc" },
        new() { MaMon = "M03", MaLoaiMon = "LM01", TenMon = "Lẩu nấm thanh đạm", LoaiMon = "Nước lẩu", MoTa = "Nước lẩu nấm ninh từ các loại củ quả thiên nhiên, vị ngọt thanh tự nhiên tốt cho sức khỏe.", DuongDanAnh = "/images/default.jpg", DonGia = 170000, SoLuong = 28, TrangThai = "InStock", BieuTuong = "🍄", Tone = "green", Tag = "Thanh đạm" },
        new() { MaMon = "M04", MaLoaiMon = "LM01", TenMon = "Lẩu gà lá é", LoaiMon = "Nước lẩu", MoTa = "Lẩu gà ta ngọt bùi kết hợp cùng lá é rừng thơm lừng, cay nồng ớt xiêm xanh.", DuongDanAnh = "/images/default.jpg", DonGia = 200000, SoLuong = 27, TrangThai = "InStock", BieuTuong = "🍗", Tone = "gold", Tag = "Đặc sản" },
        new() { MaMon = "M05", MaLoaiMon = "LM02", TenMon = "Bò Mỹ thái lát", LoaiMon = "Thịt & Hải sản nhúng lẩu", MoTa = "Thịt ba chỉ bò Mỹ thượng hạng thái lát mỏng đều, vân mỡ đan xen mềm tan.", DuongDanAnh = "/images/default.jpg", DonGia = 120000, SoLuong = 52, TrangThai = "InStock", BieuTuong = "🥩", Tone = "red", Tag = "Yêu thích" },
        new() { MaMon = "M06", MaLoaiMon = "LM02", TenMon = "Tôm sú tươi", LoaiMon = "Thịt & Hải sản nhúng lẩu", MoTa = "Tôm sú tươi rói đánh bắt trong ngày, thịt chắc ngọt giòn sần sật.", DuongDanAnh = "/images/default.jpg", DonGia = 140000, SoLuong = 46, TrangThai = "InStock", BieuTuong = "🍤", Tone = "gold", Tag = "Tươi sống" },
        new() { MaMon = "M07", MaLoaiMon = "LM02", TenMon = "Viên thả lẩu thập cẩm", LoaiMon = "Thịt & Hải sản nhúng lẩu", MoTa = "Bộ sưu tập viên thả lẩu cao cấp: cá viên trứng muối, phô mai tan chảy, bò viên gân.", DuongDanAnh = "/images/default.jpg", DonGia = 70000, SoLuong = 76, TrangThai = "InStock", BieuTuong = "🧀", Tone = "gold" },
        new() { MaMon = "M08", MaLoaiMon = "LM02", TenMon = "Ba chỉ heo thái mỏng", LoaiMon = "Thịt & Hải sản nhúng lẩu", MoTa = "Ba chỉ heo sạch chuẩn VietGAP, thái lát mỏng nhúng lẩu vừa chín tới ngọt thơm.", DuongDanAnh = "/images/default.jpg", DonGia = 90000, SoLuong = 56, TrangThai = "InStock", BieuTuong = "🥓", Tone = "red" },
        new() { MaMon = "M09", MaLoaiMon = "LM03", TenMon = "Rau lẩu thập cẩm", LoaiMon = "Rau & Nấm", MoTa = "Đĩa rau tổng hợp gồm cải thảo non, cải cúc tần ô, rau muống và hoa chuối bào.", DuongDanAnh = "/images/default.jpg", DonGia = 50000, SoLuong = 95, TrangThai = "InStock", BieuTuong = "🥬", Tone = "green" },
        new() { MaMon = "M10", MaLoaiMon = "LM03", TenMon = "Nấm thập cẩm", LoaiMon = "Rau & Nấm", MoTa = "Nấm kim châm trắng muốt, nấm đùi gà giòn ngọt, nấm hương tươi và nấm đông cô.", DuongDanAnh = "/images/default.jpg", DonGia = 60000, SoLuong = 77, TrangThai = "InStock", BieuTuong = "🍄", Tone = "green" },
        new() { MaMon = "M11", MaLoaiMon = "LM04", TenMon = "Mì Udon", LoaiMon = "Mì, Bún ăn kèm", MoTa = "Sợi mì Udon Nhật Bản dai mềm, hút trọn vị nước lẩu đậm đà.", DuongDanAnh = "/images/default.jpg", DonGia = 25000, SoLuong = 117, TrangThai = "InStock", BieuTuong = "🍜", Tone = "gold" },
        new() { MaMon = "M12", MaLoaiMon = "LM04", TenMon = "Bún tươi", LoaiMon = "Mì, Bún ăn kèm", MoTa = "Bún tươi sợi nhỏ làm từ gạo sạch nguyên chất, làm mới mỗi sáng.", DuongDanAnh = "/images/default.jpg", DonGia = 15000, SoLuong = 146, TrangThai = "InStock", BieuTuong = "🥣", Tone = "gold" },
        new() { MaMon = "M13", MaLoaiMon = "LM05", TenMon = "Trà đào cam sả", LoaiMon = "Đồ uống & Tráng miệng", MoTa = "Trà đen ủ lạnh kết hợp đào miếng giòn ngọt, cam vàng tươi và hương sả thơm mát.", DuongDanAnh = "/images/default.jpg", DonGia = 40000, SoLuong = 93, TrangThai = "InStock", BieuTuong = "🍹", Tone = "gold", Tag = "Giải khát" },
        new() { MaMon = "M14", MaLoaiMon = "LM05", TenMon = "Chè khúc bạch", LoaiMon = "Đồ uống & Tráng miệng", MoTa = "Khúc bạch béo ngậy vị sữa hạnh nhân, vải thiều ngọt lịm cùng hạt hạnh nhân giòn bùi.", DuongDanAnh = "/images/default.jpg", DonGia = 35000, SoLuong = 55, TrangThai = "InStock", BieuTuong = "🍧", Tone = "green" },
        new() { MaMon = "M15", MaLoaiMon = "LM05", TenMon = "Nước ép cam", LoaiMon = "Đồ uống & Tráng miệng", MoTa = "Cam sành vắt tươi nguyên chất 100%, giàu vitamin C sảng khoái.", DuongDanAnh = "/images/default.jpg", DonGia = 40000, SoLuong = 0, TrangThai = "OutOfStock", BieuTuong = "🍊", Tone = "gold", Tag = "Tạm hết" }
    };

    private static readonly Dictionary<string, string> FallbackCategories = new()
    {
        { "LM01", "Nước lẩu" },
        { "LM02", "Thịt & Hải sản nhúng lẩu" },
        { "LM03", "Rau & Nấm" },
        { "LM04", "Mì, Bún ăn kèm" },
        { "LM05", "Đồ uống & Tráng miệng" }
    };

    // Cached state for synchronous access
    private List<MonAnViewModel> _cachedDishes = new(FallbackMenu);
    private Dictionary<string, string> _cachedCategoryMap = new(FallbackCategories);

    public MonAnService(IHttpClientFactory httpClientFactory, ILogger<MonAnService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<List<LoaiMonItemViewModel>> GetCategoriesAsync()
    {
        var categoryMap = await EnsureCategoriesLoadedAsync();
        var allDishes = await GetAllAsync();

        var result = new List<LoaiMonItemViewModel>
        {
            new() { MaLoaiMon = "all", TenLoaiMon = "Tất cả món", SoLuongMon = allDishes.Count }
        };

        foreach (var kvp in categoryMap)
        {
            var count = allDishes.Count(d => d.MaLoaiMon == kvp.Key || d.LoaiMon == kvp.Value);
            result.Add(new LoaiMonItemViewModel
            {
                MaLoaiMon = kvp.Key,
                TenLoaiMon = kvp.Value,
                SoLuongMon = count
            });
        }

        return result;
    }

    public async Task<List<MonAnViewModel>> GetAllAsync(string? maLoai = null, string? search = null, string? sort = null)
    {
        var categoryMap = await EnsureCategoriesLoadedAsync();

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            string endpoint = "/api/MonAn";

            if (!string.IsNullOrWhiteSpace(maLoai) && maLoai != "all" && maLoai != "Tất cả")
            {
                // Prefer TheoLoai endpoint if no search, or pass query params
                if (string.IsNullOrWhiteSpace(search))
                {
                    endpoint = $"/api/MonAn/TheoLoai/{Uri.EscapeDataString(maLoai)}";
                }
                else
                {
                    endpoint = $"/api/MonAn?maLoai={Uri.EscapeDataString(maLoai)}&search={Uri.EscapeDataString(search)}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                endpoint = $"/api/MonAn?search={Uri.EscapeDataString(search)}";
            }
            
            if (!string.IsNullOrWhiteSpace(sort))
            {
                endpoint += endpoint.Contains("?") ? $"&sort={sort}" : $"?sort={sort}";
            }

            var response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                var apiDishes = await response.Content.ReadFromJsonAsync<List<ApiMonAnDto>>();
                if (apiDishes != null && apiDishes.Count > 0)
                {
                    var viewModels = apiDishes.Select(dto => MapToViewModel(dto, categoryMap)).ToList();
                    _cachedDishes = viewModels;
                    return viewModels;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kết nối đến API MonAn, chuyển sang sử dụng dữ liệu cục bộ.");
        }

        // Fallback filter
        IEnumerable<MonAnViewModel> filtered = FallbackMenu;

        if (!string.IsNullOrWhiteSpace(maLoai) && maLoai != "all" && maLoai != "Tất cả")
        {
            filtered = filtered.Where(d =>
                string.Equals(d.MaLoaiMon, maLoai, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(d.LoaiMon, maLoai, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var clean = search.Trim();
            filtered = filtered.Where(d =>
                d.TenMon.Contains(clean, StringComparison.OrdinalIgnoreCase) ||
                d.MoTa.Contains(clean, StringComparison.OrdinalIgnoreCase));
        }

        var result = filtered.Where(d => d.TrangThai != "OutOfStock").ToList();
        
        if (sort == "asc") result = result.OrderBy(x => x.DonGiaSauGiam).ToList();
        else if (sort == "desc") result = result.OrderByDescending(x => x.DonGiaSauGiam).ToList();
        
        _cachedDishes = result;
        return result;
    }

    public async Task<MonAnViewModel?> GetByIdAsync(string id)
    {
        var categoryMap = await EnsureCategoriesLoadedAsync();

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/MonAn/{Uri.EscapeDataString(id)}");
            if (response.IsSuccessStatusCode)
            {
                var apiDish = await response.Content.ReadFromJsonAsync<ApiMonAnDto>();
                if (apiDish != null)
                {
                    return MapToViewModel(apiDish, categoryMap);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi lấy chi tiết món từ API, tìm trong bộ nhớ cục bộ.");
        }

        return FallbackMenu.FirstOrDefault(d => string.Equals(d.MaMon, id, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<Dictionary<string, string>> EnsureCategoriesLoadedAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/LoaiMon");
            if (response.IsSuccessStatusCode)
            {
                var apiCategories = await response.Content.ReadFromJsonAsync<List<ApiLoaiMonDto>>();
                if (apiCategories != null && apiCategories.Count > 0)
                {
                    var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var c in apiCategories)
                    {
                        if (!string.IsNullOrEmpty(c.MaLoaiMon) && !string.IsNullOrEmpty(c.TenLoaiMon))
                        {
                            map[c.MaLoaiMon] = c.TenLoaiMon;
                        }
                    }
                    _cachedCategoryMap = map;
                    return map;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kết nối đến API LoaiMon, sử dụng danh mục mặc định.");
        }

        return _cachedCategoryMap;
    }

    private static MonAnViewModel MapToViewModel(ApiMonAnDto dto, Dictionary<string, string> categoryMap)
    {
        string loaiTen = "Món ăn";
        if (!string.IsNullOrEmpty(dto.MaLoaiMon) && categoryMap.TryGetValue(dto.MaLoaiMon, out var name))
        {
            loaiTen = name;
        }

        var fallbackMatch = FallbackMenu.FirstOrDefault(f => string.Equals(f.MaMon, dto.MaMon, StringComparison.OrdinalIgnoreCase));

        string icon = fallbackMatch?.BieuTuong ?? GuessIcon(dto.TenMon, loaiTen);
        string tone = fallbackMatch?.Tone ?? GuessTone(loaiTen);
        string tag = fallbackMatch?.Tag ?? (dto.SoLuong <= 0 ? "Tạm hết" : "");

        string img = dto.DuongDanAnh ?? "";
        if (string.IsNullOrWhiteSpace(img))
        {
            img = fallbackMatch?.DuongDanAnh ?? "";
        }

        return new MonAnViewModel
        {
            MaMon = dto.MaMon ?? "",
            MaLoaiMon = dto.MaLoaiMon,
            TenMon = dto.TenMon ?? "",
            LoaiMon = loaiTen,
            MoTa = dto.MoTa ?? "Món ăn thơm ngon, chuẩn bị nóng hổi mỗi ngày từ nhà hàng Lẩu Phố.",
            DuongDanAnh = img,
            DonGia = (decimal)(dto.DonGia ?? 0),
            DonGiaSauGiam = (decimal)(dto.DonGiaSauGiam ?? dto.DonGia ?? 0),
            SoLuong = dto.SoLuong ?? 0,
            TrangThai = dto.TrangThai ?? "InStock",
            BieuTuong = icon,
            Tag = tag,
            Tone = tone
        };
    }

    private static string GuessIcon(string? name, string? category)
    {
        var text = $"{name} {category}".ToLower();
        if (text.Contains("bò")) return "🥩";
        if (text.Contains("heo")) return "🥓";
        if (text.Contains("gà")) return "🍗";
        if (text.Contains("tôm")) return "🍤";
        if (text.Contains("hải sản")) return "🐙";
        if (text.Contains("lẩu")) return "🥘";
        if (text.Contains("rau")) return "🥬";
        if (text.Contains("nấm")) return "🍄";
        if (text.Contains("mì") || text.Contains("bún")) return "🍜";
        if (text.Contains("trà") || text.Contains("nước") || text.Contains("cam")) return "🍹";
        if (text.Contains("chè") || text.Contains("tráng miệng")) return "🍧";
        return "🍲";
    }

    private static string GuessTone(string? category)
    {
        var text = (category ?? "").ToLower();
        if (text.Contains("thịt") || text.Contains("nước lẩu")) return "red";
        if (text.Contains("rau") || text.Contains("nấm")) return "green";
        return "gold";
    }

    // Synchronous helpers for compatibility with GioHangService and legacy calls
    public IReadOnlyList<MonAnViewModel> GetAll() => _cachedDishes.Count > 0 ? _cachedDishes : FallbackMenu;
    public IReadOnlyList<string> GetCategories() => new[] { "Tất cả", "Nước lẩu", "Thịt & Hải sản nhúng lẩu", "Rau & Nấm", "Mì, Bún ăn kèm", "Đồ uống & Tráng miệng" };
    public MonAnViewModel? Find(string id)
    {
        return _cachedDishes.FirstOrDefault(x => string.Equals(x.MaMon, id, StringComparison.OrdinalIgnoreCase))
            ?? FallbackMenu.FirstOrDefault(x => string.Equals(x.MaMon, id, StringComparison.OrdinalIgnoreCase));
    }
}

// Internal DTOs for deserializing API responses safely
internal class ApiMonAnDto
{
    public string? MaMon { get; set; }
    public string? MaLoaiMon { get; set; }
    public string? TenMon { get; set; }
    public string? MoTa { get; set; }
    public string? DuongDanAnh { get; set; }
    public double? DonGia { get; set; }
    public double? DonGiaSauGiam { get; set; }
    public int? SoLuong { get; set; }
    public string? TrangThai { get; set; }
}

internal class ApiLoaiMonDto
{
    public string? MaLoaiMon { get; set; }
    public string? TenLoaiMon { get; set; }
}
