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
            var d = _monAnService.Find(it.MaMon);
            if (d != null)
            {
                if (string.IsNullOrWhiteSpace(it.DuongDanAnh) || it.DuongDanAnh == "/images/default.jpg")
                {
                    if (!string.IsNullOrWhiteSpace(d.DuongDanAnh))
                        it.DuongDanAnh = d.DuongDanAnh;
                }
                if (string.IsNullOrWhiteSpace(it.MaLoaiMon))
                {
                    it.MaLoaiMon = d.MaLoaiMon;
                }
                it.DonGiaGoc = d.DonGia;
                it.DonGiaSauGiam = (d.DonGiaSauGiam > 0 && d.DonGiaSauGiam < d.DonGia) ? d.DonGiaSauGiam : d.DonGia;
                it.DonGia = it.DonGiaSauGiam;
            }
        }

        return BuildCart(session, cart);
    }

    /// <summary>
    /// Dựng view model giỏ hàng và tính giảm giá voucher chỉ trên các món được chọn và còn khả dụng.
    /// </summary>
    private GioHangViewModel BuildCart(ISession session, List<GioHangItemViewModel> cart)
    {
        var voucher = session.GetString(VoucherKey);
        var selected = cart.Where(x => x.DuocChon && x.KhaDung).ToList();
        var subtotal = selected.Sum(x => x.ThanhTien);
        var discount = string.IsNullOrWhiteSpace(voucher) ? 0m : CalculateDiscountForCart(session, subtotal);

        return new GioHangViewModel
        {
            Items = cart,
            MaVoucher = voucher,
            GiamGia = discount
        };
    }

    /// <summary>
    /// Lấy giỏ hàng kèm trạng thái khả dụng mới nhất của từng món (hết hàng / ngừng kinh doanh) từ API.
    /// </summary>
    public async Task<GioHangViewModel> GetWithAvailabilityAsync(ISession session, string? customerId = null)
    {
        var cart = Get(session, customerId).Items;
        foreach (var it in cart)
        {
            var dish = await _monAnService.GetByIdAsync(it.MaMon);
            if (dish == null)
            {
                it.KhaDung = false;
                it.TrangThaiMon = "Ngừng kinh doanh";
            }
            else
            {
                it.KhaDung = dish.ConHang;
                it.TrangThaiMon = dish.ConHang ? null : (dish.NgungKinhDoanh ? "Ngừng kinh doanh" : "Hết hàng");
                it.DonGiaGoc = dish.DonGia;
                it.DonGiaSauGiam = (dish.DonGiaSauGiam > 0 && dish.DonGiaSauGiam < dish.DonGia) ? dish.DonGiaSauGiam : dish.DonGia;
                it.DonGia = it.DonGiaSauGiam;
                if (!string.IsNullOrWhiteSpace(dish.DuongDanAnh))
                    it.DuongDanAnh = dish.DuongDanAnh;
                if (!string.IsNullOrWhiteSpace(dish.MaLoaiMon))
                    it.MaLoaiMon = dish.MaLoaiMon;
            }
        }
        return BuildCart(session, cart);
    }

    public void SetSelected(ISession session, string maMon, bool chon)
    {
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => string.Equals(x.MaMon, maMon, StringComparison.OrdinalIgnoreCase));
        if (item is null) return;
        item.DuocChon = chon;
        Save(session, cart);
    }

    public void SetAllSelected(ISession session, bool chon)
    {
        var cart = Get(session).Items;
        foreach (var it in cart) it.DuocChon = chon;
        Save(session, cart);
    }

    /// <summary>
    /// Xóa các món đã đặt khỏi giỏ hàng (giữ lại các món chưa chọn) và gỡ voucher đã dùng.
    /// </summary>
    public void RemoveItems(ISession session, IEnumerable<string> maMons, string? customerId = null)
    {
        var set = new HashSet<string>(maMons, StringComparer.OrdinalIgnoreCase);
        var cart = Get(session, customerId).Items;
        cart.RemoveAll(x => set.Contains(x.MaMon));

        if (cart.Count == 0)
        {
            Clear(session, customerId);
            return;
        }

        session.Remove(VoucherKey);
        session.Remove(VoucherInfoKey);
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
        customerId ??= session.GetString("MaKhachHang");
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            SaveCustomerCart(customerId, cart);
        }
    }

    public void Add(ISession session, string maMon, int quantity)
    {
        var dish = _monAnService.Find(maMon);
        if (dish is null || !dish.ConHang) return;
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => x.MaMon == maMon);
        int maxQty = dish.SoLuong > 0 ? Math.Min(99, dish.SoLuong) : 99;

        decimal donGiaGoc = dish.DonGia;
        decimal donGiaSauGiam = (dish.DonGiaSauGiam > 0 && dish.DonGiaSauGiam < dish.DonGia) ? dish.DonGiaSauGiam : dish.DonGia;

        if (item is null)
        {
            cart.Add(new GioHangItemViewModel
            {
                MaMon = dish.MaMon,
                MaLoaiMon = dish.MaLoaiMon,
                TenMon = dish.TenMon,
                DuongDanAnh = dish.DuongDanAnh,
                BieuTuong = dish.BieuTuong,
                DonGiaGoc = donGiaGoc,
                DonGiaSauGiam = donGiaSauGiam,
                DonGia = donGiaSauGiam,
                SoLuong = Math.Clamp(quantity, 1, maxQty),
                DuocChon = true
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(item.DuongDanAnh) || item.DuongDanAnh == "/images/default.jpg")
                item.DuongDanAnh = dish.DuongDanAnh;
            item.DonGiaGoc = donGiaGoc;
            item.DonGiaSauGiam = donGiaSauGiam;
            item.DonGia = donGiaSauGiam;
            item.SoLuong = Math.Min(maxQty, item.SoLuong + Math.Clamp(quantity, 1, maxQty));
            item.DuocChon = true;
        }
        Save(session, cart);
    }

    public void Remove(ISession session, string maMon, string? customerId = null)
    {
        if (string.IsNullOrWhiteSpace(maMon)) return;
        customerId ??= session.GetString("MaKhachHang");
        var cart = Get(session, customerId).Items;
        var countBefore = cart.Count;
        cart.RemoveAll(x => string.Equals(x.MaMon, maMon, StringComparison.OrdinalIgnoreCase));
        if (cart.Count < countBefore)
        {
            if (cart.Count == 0)
            {
                Clear(session, customerId);
                return;
            }
            Save(session, cart);
        }
    }

    public void RemoveUnavailableItems(ISession session, string? customerId = null)
    {
        customerId ??= session.GetString("MaKhachHang");
        var cartModel = GetWithAvailabilityAsync(session, customerId).GetAwaiter().GetResult();
        var unavail = cartModel.Items.Where(x => !x.KhaDung).Select(x => x.MaMon).ToList();
        if (unavail.Count > 0)
        {
            RemoveItems(session, unavail, customerId);
        }
    }

    public void SetQuantity(ISession session, string maMon, int quantity)
    {
        if (string.IsNullOrWhiteSpace(maMon)) return;
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => string.Equals(x.MaMon, maMon, StringComparison.OrdinalIgnoreCase));
        if (item is not null)
        {
            var dish = _monAnService.Find(maMon);
            int maxQty = (dish != null && dish.SoLuong > 0) ? Math.Min(99, dish.SoLuong) : 99;
            if (quantity <= 0)
            {
                cart.Remove(item);
                if (cart.Count == 0)
                {
                    Clear(session);
                    return;
                }
            }
            else
            {
                item.SoLuong = Math.Min(quantity, maxQty);
            }
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
                return (vouchers ?? new List<VoucherViewModel>())
                    .Where(v => string.IsNullOrEmpty(v.PhamViApDung) || string.Equals(v.PhamViApDung, "TOAN_DON", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(v => v.NgayBatDau ?? DateOnly.MinValue)
                    .ThenByDescending(v => v.MaKmvoucher)
                    .ToList();
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
        var cart = await GetWithAvailabilityAsync(session, customerId);
        var subtotal = cart.TamTinh;
        var selectedItems = cart.ItemsDaChon;

        if (selectedItems.Count == 0)
        {
            session.SetString(VoucherKey, clean);
            return (true, $"Đã lưu mã {clean}. Hãy chọn món ăn trong giỏ để nhận ưu đãi!", 0m);
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
                    if (string.Equals(apiResult.PhamViApDung, "THEO_MON", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(apiResult.PhamViApDung, "THEO_LOAI", StringComparison.OrdinalIgnoreCase))
                    {
                        session.Remove(VoucherKey);
                        session.Remove(VoucherInfoKey);
                        return (false, "Các ưu đãi theo món và loại món đã được hệ thống tự động chọn mức giảm cao nhất áp thẳng vào giá món ăn. Tại giỏ hàng chỉ áp dụng voucher giảm theo toàn đơn hàng.", 0m);
                    }

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
                    var realDiscount = ComputeScopedDiscount(voucherInfo, subtotal);
                    if (realDiscount <= 0)
                    {
                        session.Remove(VoucherKey);
                        session.Remove(VoucherInfoKey);
                        return (false, BuildNotApplicableMessage(voucherInfo, subtotal), 0m);
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

    private decimal CalculateDiscountForCart(ISession session, decimal subtotal)
    {
        if (subtotal <= 0) return 0m;

        var infoJson = session.GetString(VoucherInfoKey);
        if (!string.IsNullOrWhiteSpace(infoJson))
        {
            try
            {
                var info = JsonSerializer.Deserialize<AppliedVoucherSessionData>(infoJson);
                if (info != null) return ComputeScopedDiscount(info, subtotal);
            }
            catch { }
        }

        return 0m;
    }

    private static decimal ComputeScopedDiscount(AppliedVoucherSessionData info, decimal subtotal)
    {
        var scope = string.IsNullOrWhiteSpace(info.PhamViApDung) ? "TOAN_DON" : info.PhamViApDung;
        if (scope != "TOAN_DON" || subtotal <= 0) return 0m;

        if (info.GiaTriDonToiThieu.HasValue && info.GiaTriDonToiThieu.Value > 0
            && subtotal < (decimal)info.GiaTriDonToiThieu.Value)
        {
            return 0m;
        }

        decimal disc = 0m;
        if (info.PhanTramGiam.HasValue && info.PhanTramGiam.Value > 0)
        {
            disc = Math.Round(subtotal * (decimal)info.PhanTramGiam.Value / 100m, 0);
            if (info.GiamToiDa.HasValue && info.GiamToiDa.Value > 0 && disc > (decimal)info.GiamToiDa.Value)
            {
                disc = (decimal)info.GiamToiDa.Value;
            }
        }
        else if (info.TienGiam.HasValue && info.TienGiam.Value > 0)
        {
            disc = (decimal)info.TienGiam.Value;
        }

        return Math.Min(disc, subtotal);
    }

    private static string BuildNotApplicableMessage(AppliedVoucherSessionData info, decimal subtotal)
    {
        if (info.GiaTriDonToiThieu.HasValue && subtotal < (decimal)info.GiaTriDonToiThieu.Value)
        {
            return $"Mã này yêu cầu đơn hàng tối thiểu {info.GiaTriDonToiThieu.Value:N0}đ (hiện có {subtotal:N0}đ).";
        }
        return "Mã ưu đãi không áp dụng được cho giỏ hàng hiện tại.";
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
