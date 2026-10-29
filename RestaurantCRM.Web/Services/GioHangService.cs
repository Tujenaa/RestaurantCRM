using System.Collections.Concurrent;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class CartCheckApiResponse
{
    public bool HopLe { get; set; }
    public string? ThongBao { get; set; }
    public List<CartItemErrorDetailDto> ChiTietLoi { get; set; } = new();
}

public class CartItemErrorDetailDto
{
    public string MaMon { get; set; } = "";
    public string? TenMon { get; set; }
    public int SoLuongYeuCau { get; set; }
    public int SoLuongKhaDung { get; set; }
    public string? LyDo { get; set; }
}

public class GioHangService
{
    private const string CartKey = "customer-cart-v1";
    private const string VoucherKey = "customer-voucher-v1";
    private const string VoucherInfoKey = "customer-voucher-info-v1";

    private readonly MonAnService _monAnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GioHangService> _logger;

    // Lưu trữ giỏ hàng bền vững theo từng tài khoản khách hàng để không bị mất khi đăng xuất / đăng nhập lại
    private static readonly ConcurrentDictionary<string, List<GioHangItemViewModel>> _persistentCustomerCarts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object _cartFileLock = new();
    private static readonly string _cartFilePath = Path.Combine(AppContext.BaseDirectory, "customer_saved_carts.json");

    static GioHangService()
    {
        try
        {
            if (File.Exists(_cartFilePath))
            {
                var json = File.ReadAllText(_cartFilePath);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, List<GioHangItemViewModel>>>(json);
                if (loaded != null)
                {
                    foreach (var kvp in loaded)
                    {
                        _persistentCustomerCarts[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch { }
    }

    private static void PersistCartsToFile()
    {
        try
        {
            lock (_cartFileLock)
            {
                var dict = new Dictionary<string, List<GioHangItemViewModel>>(_persistentCustomerCarts, StringComparer.OrdinalIgnoreCase);
                var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_cartFilePath, json);
            }
        }
        catch { }
    }

    public static void SaveCustomerCart(string customerId, List<GioHangItemViewModel> cart)
    {
        if (string.IsNullOrWhiteSpace(customerId)) return;
        customerId = customerId.Trim();
        _persistentCustomerCarts[customerId] = new List<GioHangItemViewModel>(cart);
        PersistCartsToFile();
    }

    public static void ClearCustomerCart(string customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId)) return;
        customerId = customerId.Trim();
        _persistentCustomerCarts.TryRemove(customerId, out _);
        PersistCartsToFile();
    }

    public void PreserveCartBeforeLogout(ISession session, string? customerId)
    {
        customerId ??= session.GetString("MaKhachHang");
        if (string.IsNullOrWhiteSpace(customerId)) return;

        var json = session.GetString(CartKey);
        if (!string.IsNullOrWhiteSpace(json))
        {
            var cart = JsonSerializer.Deserialize<List<GioHangItemViewModel>>(json) ?? new();
            if (cart.Count > 0)
            {
                SaveCustomerCart(customerId, cart);
            }
        }
    }

    public void RestoreCartOnLogin(ISession session, string? customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId)) return;
        customerId = customerId.Trim();

        var guestJson = session.GetString(CartKey);
        var guestCart = string.IsNullOrWhiteSpace(guestJson)
            ? new List<GioHangItemViewModel>()
            : JsonSerializer.Deserialize<List<GioHangItemViewModel>>(guestJson) ?? new();

        _persistentCustomerCarts.TryGetValue(customerId, out var savedCart);
        savedCart ??= new List<GioHangItemViewModel>();

        var merged = new List<GioHangItemViewModel>(savedCart);
        foreach (var guestItem in guestCart)
        {
            var existing = merged.FirstOrDefault(x => string.Equals(x.MaMon, guestItem.MaMon, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.SoLuong = Math.Min(99, existing.SoLuong + guestItem.SoLuong);
            }
            else
            {
                merged.Add(guestItem);
            }
        }

        if (merged.Count > 0)
        {
            session.SetString(CartKey, JsonSerializer.Serialize(merged));
            SaveCustomerCart(customerId, merged);
        }
    }

    public GioHangService(
        MonAnService monAnService,
        IHttpClientFactory httpClientFactory,
        ILogger<GioHangService> logger)
    {
        _monAnService = monAnService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public GioHangViewModel Get(ISession session, string? customerId = null)
    {
        customerId ??= session.GetString("MaKhachHang");
        var json = session.GetString(CartKey);
        List<GioHangItemViewModel> cart;

        if (string.IsNullOrWhiteSpace(json))
        {
            if (!string.IsNullOrWhiteSpace(customerId) && _persistentCustomerCarts.TryGetValue(customerId.Trim(), out var saved) && saved.Count > 0)
            {
                cart = new List<GioHangItemViewModel>(saved);
                session.SetString(CartKey, JsonSerializer.Serialize(cart));
            }
            else
            {
                cart = new List<GioHangItemViewModel>();
            }
        }
        else
        {
            cart = JsonSerializer.Deserialize<List<GioHangItemViewModel>>(json) ?? new();
            if (cart.Count == 0 && !string.IsNullOrWhiteSpace(customerId) && _persistentCustomerCarts.TryGetValue(customerId.Trim(), out var saved) && saved.Count > 0)
            {
                cart = new List<GioHangItemViewModel>(saved);
                session.SetString(CartKey, JsonSerializer.Serialize(cart));
            }
        }

        foreach (var it in cart)
        {
            if (string.IsNullOrWhiteSpace(it.DuongDanAnh) || it.DuongDanAnh == "/images/default.jpg")
            {
                var d = _monAnService.Find(it.MaMon);
                if (d != null && !string.IsNullOrWhiteSpace(d.DuongDanAnh))
                    it.DuongDanAnh = d.DuongDanAnh;
            }
            if (string.IsNullOrWhiteSpace(it.MaLoaiMon))
            {
                it.MaLoaiMon = _monAnService.Find(it.MaMon)?.MaLoaiMon;
            }
        }

        var voucher = session.GetString(VoucherKey);
        var subtotal = cart.Sum(x => x.ThanhTien);
        var discount = string.IsNullOrWhiteSpace(voucher) ? 0m : CalculateDiscountForCart(session, cart, subtotal);

        return new GioHangViewModel
        {
            Items = cart,
            MaVoucher = voucher,
            GiamGia = discount
        };
    }

    public void Add(ISession session, string maMon, int quantity)
    {
        var dish = _monAnService.Find(maMon);
        if (dish is null || !dish.ConHang) return;
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => x.MaMon == maMon);
        int maxQty = dish.SoLuong > 0 ? Math.Min(99, dish.SoLuong) : 99;

        if (item is null)
        {
            cart.Add(new GioHangItemViewModel
            {
                MaMon = dish.MaMon,
                MaLoaiMon = dish.MaLoaiMon,
                TenMon = dish.TenMon,
                DuongDanAnh = dish.DuongDanAnh,
                BieuTuong = dish.BieuTuong,
                DonGia = (dish.DonGiaSauGiam > 0 && dish.DonGiaSauGiam < dish.DonGia) ? dish.DonGiaSauGiam : dish.DonGia,
                SoLuong = Math.Clamp(quantity, 1, maxQty)
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(item.DuongDanAnh) || item.DuongDanAnh == "/images/default.jpg")
                item.DuongDanAnh = dish.DuongDanAnh;
            item.SoLuong = Math.Min(maxQty, item.SoLuong + Math.Clamp(quantity, 1, maxQty));
        }
        Save(session, cart);
    }

    public void SetQuantity(ISession session, string maMon, int quantity)
    {
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => x.MaMon == maMon);
        if (item is not null)
        {
            var dish = _monAnService.Find(maMon);
            int maxQty = (dish != null && dish.SoLuong > 0) ? Math.Min(99, dish.SoLuong) : 99;
            if (quantity <= 0) cart.Remove(item);
            else item.SoLuong = Math.Min(quantity, maxQty);
        }
        Save(session, cart);
    }

    public async Task<List<VoucherViewModel>> GetAvailableVouchersAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/KhuyenMai/Vouchers");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var vouchers = await response.Content.ReadFromJsonAsync<List<VoucherViewModel>>(options);
                return vouchers ?? new List<VoucherViewModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kết nối API /api/KhuyenMai/Vouchers để lấy danh sách voucher.");
        }
        return new List<VoucherViewModel>();
    }

    public async Task<(bool Success, string Message, decimal Discount)> ApplyVoucherAsync(ISession session, string? code, string? customerId = null)
    {
        customerId ??= session.GetString("MaKhachHang");

        if (string.IsNullOrWhiteSpace(code))
        {
            session.Remove(VoucherKey);
            session.Remove(VoucherInfoKey);
            return (true, "Đã gỡ mã ưu đãi khỏi đơn hàng.", 0m);
        }

        var clean = code.Trim().ToUpperInvariant();
        var cart = Get(session, customerId);
        var subtotal = cart.TamTinh;

        if (cart.Items.Count == 0)
        {
            session.SetString(VoucherKey, clean);
            return (true, $"Đã lưu mã {clean}. Hãy chọn thêm món ăn vào giỏ để nhận ưu đãi!", 0m);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/KhuyenMai/KiemTraVoucher?code={Uri.EscapeDataString(clean)}&subtotal={(double)subtotal}");
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResult = await response.Content.ReadFromJsonAsync<KiemTraVoucherApiResponse>(options);
                if (apiResult != null && apiResult.Valid)
                {
                    var savedCode = !string.IsNullOrWhiteSpace(apiResult.MaVoucher)
                        ? apiResult.MaVoucher
                        : (!string.IsNullOrWhiteSpace(apiResult.MaKmvoucher) ? apiResult.MaKmvoucher : clean);

                    var voucherInfo = new AppliedVoucherSessionData
                    {
                        Code = savedCode,
                        MaKmvoucher = apiResult.MaKmvoucher,
                        TenChuongTrinh = apiResult.TenChuongTrinh,
                        PhanTramGiam = apiResult.PhanTramGiam,
                        TienGiam = apiResult.TienGiam,
                        GiaTriDonToiThieu = apiResult.GiaTriDonToiThieu,
                        GiamToiDa = apiResult.GiamToiDa,
                        PhamViApDung = apiResult.PhamViApDung,
                        MaLoaiMon = apiResult.MaLoaiMon,
                        MaMon = apiResult.MaMon
                    };

                    // API không biết giỏ hàng -> tự tính giảm giá theo đúng phạm vi áp dụng
                    var realDiscount = ComputeScopedDiscount(voucherInfo, cart.Items, subtotal);
                    if (realDiscount <= 0)
                    {
                        session.Remove(VoucherKey);
                        session.Remove(VoucherInfoKey);
                        return (false, BuildNotApplicableMessage(voucherInfo, subtotal, cart.Items), 0m);
                    }

                    voucherInfo.LastCalculatedDiscount = realDiscount;
                    session.SetString(VoucherKey, savedCode);
                    session.SetString(VoucherInfoKey, JsonSerializer.Serialize(voucherInfo));

                    var msg = $"Áp dụng voucher {savedCode} thành công! Giảm ngay {realDiscount:N0}đ.";

                    return (true, msg, realDiscount);
                }
                else
                {
                    session.Remove(VoucherKey);
                    session.Remove(VoucherInfoKey);
                    return (false, apiResult?.Message ?? "Mã ưu đãi không hợp lệ hoặc đã hết hạn.", 0m);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API kiểm tra voucher {Code}.", clean);
        }

        session.Remove(VoucherKey);
        session.Remove(VoucherInfoKey);
        return (false, "Không thể kết nối đến hệ thống kiểm tra voucher hoặc mã không hợp lệ.", 0m);
    }

    public (bool Success, string Message, decimal Discount) ApplyVoucher(ISession session, string? code, string? customerId = null)
    {
        return ApplyVoucherAsync(session, code, customerId).GetAwaiter().GetResult();
    }

    public async Task<(bool IsValid, string? ErrorMessage)> CheckCartAvailabilityAsync(IReadOnlyList<GioHangItemViewModel> items)
    {
        if (items == null || items.Count == 0)
        {
            return (true, null);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var payload = new
            {
                Items = items.Select(i => new { MaMon = i.MaMon, SoLuong = i.SoLuong }).ToList()
            };

            var response = await client.PostAsJsonAsync("/api/HoaDon/KiemTraGioHang", payload);
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = await response.Content.ReadFromJsonAsync<CartCheckApiResponse>(options);
                if (result != null && !result.HopLe)
                {
                    return (false, !string.IsNullOrWhiteSpace(result.ThongBao)
                        ? result.ThongBao
                        : "Một số món ăn trong giỏ hàng hiện không đủ số lượng hoặc đã ngừng phục vụ.");
                }

                return (true, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể kết nối đến API /api/HoaDon/KiemTraGioHang.");
        }

        return (true, null);
    }

    private decimal CalculateDiscountForCart(ISession session, List<GioHangItemViewModel> items, decimal subtotal)
    {
        if (subtotal <= 0) return 0m;

        var infoJson = session.GetString(VoucherInfoKey);
        if (!string.IsNullOrWhiteSpace(infoJson))
        {
            try
            {
                var info = JsonSerializer.Deserialize<AppliedVoucherSessionData>(infoJson);
                if (info != null) return ComputeScopedDiscount(info, items, subtotal);
            }
            catch { }
        }

        return 0m;
    }

    /// <summary>
    /// Tính tiền giảm chỉ trên các dòng giỏ hàng thuộc phạm vi voucher:
    /// TOAN_DON = cả giỏ, THEO_LOAI = món cùng loại, THEO_MON = đúng món đó.
    /// </summary>
    private static decimal GetScopedBase(AppliedVoucherSessionData info, List<GioHangItemViewModel> items, decimal subtotal)
    {
        var scope = string.IsNullOrWhiteSpace(info.PhamViApDung) ? "TOAN_DON" : info.PhamViApDung;
        if (scope == "THEO_MON")
        {
            if (string.IsNullOrWhiteSpace(info.MaMon)) return 0m;
            return items.Where(x => string.Equals(x.MaMon, info.MaMon, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.ThanhTien);
        }
        if (scope == "THEO_LOAI")
        {
            if (string.IsNullOrWhiteSpace(info.MaLoaiMon)) return 0m;
            return items.Where(x => string.Equals(x.MaLoaiMon, info.MaLoaiMon, StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.ThanhTien);
        }
        return subtotal;
    }

    private static decimal ComputeScopedDiscount(AppliedVoucherSessionData info, List<GioHangItemViewModel> items, decimal subtotal)
    {
        var baseAmount = GetScopedBase(info, items, subtotal);
        if (baseAmount <= 0) return 0m;

        if (info.GiaTriDonToiThieu.HasValue && info.GiaTriDonToiThieu.Value > 0
            && baseAmount < (decimal)info.GiaTriDonToiThieu.Value)
        {
            return 0m;
        }

        decimal disc = 0m;
        if (info.PhanTramGiam.HasValue && info.PhanTramGiam.Value > 0)
        {
            disc = Math.Round(baseAmount * (decimal)info.PhanTramGiam.Value / 100m, 0);
            if (info.GiamToiDa.HasValue && info.GiamToiDa.Value > 0 && disc > (decimal)info.GiamToiDa.Value)
            {
                disc = (decimal)info.GiamToiDa.Value;
            }
        }
        else if (info.TienGiam.HasValue && info.TienGiam.Value > 0)
        {
            disc = (decimal)info.TienGiam.Value;
        }

        return Math.Min(disc, baseAmount);
    }

    private static string BuildNotApplicableMessage(AppliedVoucherSessionData info, decimal subtotal, List<GioHangItemViewModel> items)
    {
        var baseAmount = GetScopedBase(info, items, subtotal);
        if (baseAmount <= 0)
        {
            return info.PhamViApDung == "THEO_LOAI"
                ? "Mã này chỉ áp dụng cho nhóm món nhất định. Giỏ hàng của bạn chưa có món thuộc nhóm đó."
                : "Mã này chỉ áp dụng cho một món nhất định. Giỏ hàng của bạn chưa có món đó.";
        }
        if (info.GiaTriDonToiThieu.HasValue && baseAmount < (decimal)info.GiaTriDonToiThieu.Value)
        {
            return $"Mã này yêu cầu giá trị áp dụng tối thiểu {info.GiaTriDonToiThieu.Value:N0}đ (hiện có {baseAmount:N0}đ).";
        }
        return "Mã ưu đãi không áp dụng được cho giỏ hàng hiện tại.";
    }

    public static decimal CalculateDiscount(string code, decimal subtotal, List<GioHangItemViewModel>? items = null, MonAnService? monAnService = null)
    {
        // Legacy fallback
        return 0m;
    }

    public void Clear(ISession session, string? customerId = null)
    {
        session.Remove(CartKey);
        session.Remove(VoucherKey);
        session.Remove(VoucherInfoKey);
        customerId ??= session.GetString("MaKhachHang");
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            ClearCustomerCart(customerId);
        }
    }

    private static void Save(ISession session, List<GioHangItemViewModel> cart)
    {
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
        var customerId = session.GetString("MaKhachHang");
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            SaveCustomerCart(customerId, cart);
        }
    }
}
