using System.Collections.Concurrent;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class VoucherItemInfo
{
    public string Code { get; set; } = "";
    public string Label { get; set; } = "";
    public decimal MinSpend { get; set; }
    public string Description { get; set; } = "";
}

public class GioHangService
{
    private const string CartKey = "customer-cart-v1";
    private const string VoucherKey = "customer-voucher-v1";
    private readonly MonAnService _monAnService;
    private readonly IHttpClientFactory _httpClientFactory;

    // Cache các mã voucher đã sử dụng phân tách tuyệt đối theo mã khách hàng: MaKhachHang -> Set<VoucherCode>
    private static readonly ConcurrentDictionary<string, HashSet<string>> _usedVouchersByCustomer = new(StringComparer.OrdinalIgnoreCase);

    // Lưu trữ giỏ hàng bền vững theo từng tài khoản khách hàng để không bao giờ bị mất khi đăng xuất / đăng nhập lại
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

    public static readonly IReadOnlyList<VoucherItemInfo> AllVouchers = new List<VoucherItemInfo>
    {
        new() { Code = "LAUPHO20", Label = "-20k", MinSpend = 0m, Description = "Giảm 20.000đ cho mọi đơn hàng" },
        new() { Code = "VOUCHER40K", Label = "-40k", MinSpend = 200000m, Description = "Giảm 40.000đ cho đơn từ 200.000đ" },
        new() { Code = "VOUCHER10", Label = "-10%", MinSpend = 0m, Description = "Giảm 10% tổng đơn hàng" },
        new() { Code = "NHOM15", Label = "-15%", MinSpend = 500000m, Description = "Giảm 15% cho đơn từ 500.000đ" }
    };

    public GioHangService(MonAnService monAnService, IHttpClientFactory httpClientFactory)
    {
        _monAnService = monAnService;
        _httpClientFactory = httpClientFactory;
    }

    public static string NormalizeVoucherCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "";
        var clean = code.Trim().ToUpperInvariant();
        return clean switch
        {
            "LAUPHO20" or "KMV04" or "KM07" or "WELCOME20" => "LAUPHO20",
            "VOUCHER40K" or "KMV02" or "KM02" => "VOUCHER40K",
            "VOUCHER10" or "KMV01" or "KM01" => "VOUCHER10",
            "NHOM15" or "KMV03" or "KM05" => "NHOM15",
            _ => clean
        };
    }

    public static HashSet<string> GetUsedVouchersStatic(ISession session, string? customerId)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return result;
        }

        customerId = customerId.Trim();

        // 1. Kiểm tra trong in-memory dictionary theo đúng customerId này
        if (_usedVouchersByCustomer.TryGetValue(customerId, out var set))
        {
            lock (set)
            {
                foreach (var v in set) result.Add(NormalizeVoucherCode(v));
            }
        }

        // 2. Kiểm tra trong Session riêng của khách hàng này
        var sessionUsed = session.GetString($"UsedVouchers_{customerId}");
        if (!string.IsNullOrWhiteSpace(sessionUsed))
        {
            foreach (var v in sessionUsed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                result.Add(NormalizeVoucherCode(v));
            }
        }

        return result;
    }

    public async Task SyncUsedVouchersFromApiAsync(ISession session, string? customerId)
    {
        if (string.IsNullOrWhiteSpace(customerId)) return;
        customerId = customerId.Trim();

        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/KhuyenMai/UsedVouchers/{Uri.EscapeDataString(customerId)}");
            if (response.IsSuccessStatusCode)
            {
                var list = await response.Content.ReadFromJsonAsync<List<string>>();
                if (list != null)
                {
                    var set = _usedVouchersByCustomer.GetOrAdd(customerId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    lock (set)
                    {
                        foreach (var v in list)
                        {
                            set.Add(NormalizeVoucherCode(v));
                        }
                    }
                    session.SetString($"UsedVouchers_{customerId}", string.Join(",", set));
                }
            }
        }
        catch
        {
            // API offline -> giữ nguyên dữ liệu từ memory/session
        }
    }

    public void MarkVoucherAsUsed(ISession session, string? customerId, string? voucherCode)
    {
        if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(voucherCode)) return;
        customerId = customerId.Trim();
        var norm = NormalizeVoucherCode(voucherCode);

        var set = _usedVouchersByCustomer.GetOrAdd(customerId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        lock (set)
        {
            set.Add(norm);
        }

        var usedInSession = session.GetString($"UsedVouchers_{customerId}");
        var currentList = string.IsNullOrWhiteSpace(usedInSession)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(usedInSession.Split(',', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

        currentList.Add(norm);
        session.SetString($"UsedVouchers_{customerId}", string.Join(",", currentList));
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
                if (d != null && !string.IsNullOrWhiteSpace(d.DuongDanAnh)) it.DuongDanAnh = d.DuongDanAnh;
            }
        }

        if (cart.Count > 0)
        {
            AutoApplyBestVoucher(session, cart, customerId);
        }

        var voucher = session.GetString(VoucherKey);
        var usedStr = session.GetString("UsedVouchers");
        if (!string.IsNullOrWhiteSpace(voucher) && IsVoucherUsed(voucher, usedStr))
        {
            session.Remove(VoucherKey);
            voucher = null;
        }

        var subtotal = cart.Sum(x => x.ThanhTien);
        var discount = string.IsNullOrWhiteSpace(voucher) ? 0m : CalculateDiscount(voucher, subtotal);
        return new GioHangViewModel { Items = cart, MaVoucher = voucher, GiamGia = discount };
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
                TenMon = dish.TenMon,
                DuongDanAnh = dish.DuongDanAnh,
                BieuTuong = dish.BieuTuong,
                DonGia = (dish.DonGiaSauGiam > 0 && dish.DonGiaSauGiam < dish.DonGia) ? dish.DonGiaSauGiam : dish.DonGia,
                SoLuong = Math.Clamp(quantity, 1, maxQty)
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(item.DuongDanAnh) || item.DuongDanAnh == "/images/default.jpg") item.DuongDanAnh = dish.DuongDanAnh;
            item.SoLuong = Math.Min(maxQty, item.SoLuong + Math.Clamp(quantity, 1, maxQty));
        }
        Save(session, cart);
        session.Remove("VoucherManuallyRemoved");
        AutoApplyBestVoucher(session, cart, session.GetString("MaKhachHang"));
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
        session.Remove("VoucherManuallyRemoved");
        AutoApplyBestVoucher(session, cart, session.GetString("MaKhachHang"));
    }

    public (bool Success, string Message, decimal Discount) ApplyVoucher(ISession session, string? code, string? customerId = null)
    {
        customerId ??= session.GetString("MaKhachHang");

        if (string.IsNullOrWhiteSpace(code))
        {
            session.Remove(VoucherKey);
            session.SetString("VoucherManuallyRemoved", "true");
            return (true, "Đã gỡ mã ưu đãi khỏi đơn hàng.", 0m);
        }

        session.Remove("VoucherManuallyRemoved");

        var clean = NormalizeVoucherCode(code);
        var used = GetUsedVouchersStatic(session, customerId);
        if (used.Contains(clean))
        {
            return (false, $"Bạn đã từng sử dụng mã {code.Trim().ToUpper()} này rồi. Mỗi khách hàng chỉ được áp dụng 1 lần.", 0m);
        }

        var cart = Get(session, customerId);
        var subtotal = cart.TamTinh;

        if (cart.Items.Count == 0)
        {
            session.SetString(VoucherKey, clean);
            return (true, $"Đã lưu mã {code.Trim().ToUpper()}. Hãy chọn thêm món ăn vào giỏ để nhận ưu đãi!", 0m);
        }

        decimal discount = CalculateDiscount(clean, subtotal);
        if (discount > 0)
        {
            session.SetString(VoucherKey, clean);
            return (true, $"Áp dụng voucher {clean} thành công! Giảm ngay {discount:N0}đ.", discount);
        }

        if (clean == "NHOM15")
        {
            return (false, $"Mã {code.Trim().ToUpper()} (giảm 15%) áp dụng cho đơn từ 500.000đ trở lên (hiện tại: {subtotal:N0}đ).", 0m);
        }

        if (clean == "VOUCHER40K")
        {
            return (false, $"Mã {code.Trim().ToUpper()} (giảm 40.000đ) áp dụng cho đơn từ 200.000đ trở lên (hiện tại: {subtotal:N0}đ).", 0m);
        }

        session.Remove(VoucherKey);
        return (false, $"Mã {code.Trim().ToUpper()} không hợp lệ hoặc chưa đủ điều kiện áp dụng.", 0m);
    }

    public static bool IsVoucherUsed(string? code, string? usedVouchersString)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(usedVouchersString)) return false;
        var clean = code.Trim().ToUpperInvariant();
        var usedList = usedVouchersString.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return usedList.Any(u => string.Equals(u.Trim(), clean, StringComparison.OrdinalIgnoreCase)
            || (clean == "VOUCHER10" && (u == "KM01" || u == "KMV01"))
            || ((clean == "KM01" || clean == "KMV01") && u == "VOUCHER10")
            || (clean == "VOUCHER40K" && (u == "KM02" || u == "KMV02"))
            || ((clean == "KM02" || clean == "KMV02") && u == "VOUCHER40K")
            || (clean == "NHOM15" && (u == "KM05" || u == "KMV03"))
            || ((clean == "KM05" || clean == "KMV03") && u == "NHOM15")
            || (clean == "LAUPHO20" && u == "KMV04")
            || (clean == "KMV04" && u == "LAUPHO20"));
    }

    public static decimal CalculateDiscount(string code, decimal subtotal)
    {
        var clean = NormalizeVoucherCode(code);
        return clean switch
        {
            "LAUPHO20" => Math.Min(subtotal, 20000m),
            "VOUCHER40K" => subtotal >= 200000m ? 40000m : 0m,
            "VOUCHER10" => Math.Round(subtotal * 0.10m, 0),
            "NHOM15" => subtotal >= 500000m ? Math.Round(subtotal * 0.15m, 0) : 0m,
            _ => 0m
        };
    }

    public void Clear(ISession session, string? customerId = null)
    {
        session.Remove(CartKey);
        session.Remove(VoucherKey);
        session.Remove("VoucherManuallyRemoved");
        customerId ??= session.GetString("MaKhachHang");
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            ClearCustomerCart(customerId);
        }
    }

    private void Save(ISession session, List<GioHangItemViewModel> cart)
    {
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
        var customerId = session.GetString("MaKhachHang");
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            SaveCustomerCart(customerId, cart);
        }
    }

    public void AutoApplyBestVoucher(ISession session, List<GioHangItemViewModel> cart, string? customerId)
    {
        var subtotal = cart.Sum(x => x.ThanhTien);
        if (subtotal == 0) return;

        if (session.GetString("VoucherManuallyRemoved") == "true") return;

        var usedVouchers = GetUsedVouchersStatic(session, customerId);

        var availableVouchers = AllVouchers
            .Where(v => !usedVouchers.Contains(v.Code))
            .ToList();

        var currentVoucher = session.GetString(VoucherKey);
        if (!string.IsNullOrWhiteSpace(currentVoucher) && usedVouchers.Contains(NormalizeVoucherCode(currentVoucher)))
        {
            session.Remove(VoucherKey);
            currentVoucher = null;
        }

        var currentDiscount = string.IsNullOrWhiteSpace(currentVoucher) ? 0m : CalculateDiscount(currentVoucher, subtotal);

        // Nếu khách hàng đã chọn một voucher hợp lệ và voucher đó vẫn còn áp dụng được (> 0đ):
        // Tuyệt đối giữ nguyên lựa chọn của khách hàng, không tự ý ghi đè sang voucher khác!
        if (!string.IsNullOrWhiteSpace(currentVoucher) && currentDiscount > 0)
        {
            return;
        }

        string? bestVoucher = null;
        decimal maxDiscount = 0m;

        foreach (var v in availableVouchers)
        {
            var d = CalculateDiscount(v.Code, subtotal);
            if (d > maxDiscount)
            {
                maxDiscount = d;
                bestVoucher = v.Code;
            }
        }

        if (maxDiscount > 0 && bestVoucher != null)
        {
            session.SetString(VoucherKey, bestVoucher);
        }
        else
        {
            session.Remove(VoucherKey);
        }
    }
}
