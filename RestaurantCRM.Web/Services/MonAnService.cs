using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class MonAnService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MonAnService> _logger;

    private static readonly Dictionary<string, string> DefaultCategoryMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "LM001", "Lẩu" },
        { "LM002", "Thịt & hải sản nhúng lẩu" },
        { "LM003", "Rau, nấm & đậu hũ" },
        { "LM004", "Mì, bún & cơm" },
        { "LM005", "Đồ uống" },
        { "LM006", "Tráng miệng" },
        { "LM007", "Khai vị & món ăn kèm" },
        { "LM008", "Viên, chả & đồ chế biến sẵn" }
    };

    // Cached state for synchronous access (chỉ lưu các món thực tế đã tải từ API)
    private List<MonAnViewModel> _cachedDishes = new();
    private Dictionary<string, string> _cachedCategoryMap = new(DefaultCategoryMap, StringComparer.OrdinalIgnoreCase);

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
                if (apiDishes != null)
                {
                    var viewModels = apiDishes
                        .Select(dto => MapToViewModel(dto, categoryMap))
                        .Where(d => d.ConHang)
                        .ToList();

                    if (string.IsNullOrWhiteSpace(maLoai) && string.IsNullOrWhiteSpace(search))
                    {
                        _cachedDishes = viewModels;
                    }
                    else if (_cachedDishes.Count == 0 && viewModels.Count > 0)
                    {
                        _cachedDishes = viewModels;
                    }

                    return viewModels;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kết nối đến API MonAn.");
        }

        return new List<MonAnViewModel>();
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
                    var vm = MapToViewModel(apiDish, categoryMap);
                    return vm.ConHang ? vm : null;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lỗi khi lấy chi tiết món từ API.");
        }

        return null;
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
                if (apiCategories != null)
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
            _logger.LogWarning(ex, "Không thể kết nối đến API LoaiMon.");
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

        string icon = GuessIcon(dto.TenMon, loaiTen);
        string tone = GuessTone(loaiTen);
        string tag = dto.SoLuong <= 0 ? "Tạm hết" : "";

        string img = dto.DuongDanAnh ?? "";
        if (string.IsNullOrWhiteSpace(img))
        {
            img = "/images/default.jpg";
        }
        else if (!img.StartsWith("/") && !img.StartsWith("http"))
        {
            img = "/images/" + img;
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
            Tone = tone,
            DiemDanhGiaTrungBinh = 5.0,
            SoLuongDanhGia = 0
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

    // Synchronous helpers for compatibility with GioHangService
    public IReadOnlyList<MonAnViewModel> GetAll() => _cachedDishes.Where(d => d.ConHang).ToList();
    public IReadOnlyList<string> GetCategories() => _cachedCategoryMap.Values.ToList();
    public MonAnViewModel? Find(string id)
    {
        var item = _cachedDishes.FirstOrDefault(x => string.Equals(x.MaMon, id, StringComparison.OrdinalIgnoreCase));
        if (item == null)
        {
            item = GetByIdAsync(id).GetAwaiter().GetResult();
            if (item != null)
            {
                _cachedDishes.Add(item);
            }
        }
        return item != null && item.ConHang ? item : null;
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
