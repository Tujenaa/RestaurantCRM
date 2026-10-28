using System.Net.Http.Json;
using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class GioHangService
{
    private const string CartKey = "customer-cart-v1";
    private const string VoucherKey = "customer-voucher-v1";
    private const string VoucherInfoKey = "customer-voucher-info-v1";

    private readonly MonAnService _monAnService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GioHangService> _logger;

    public GioHangService(
        MonAnService monAnService,
        IHttpClientFactory httpClientFactory,
        ILogger<GioHangService> logger)
    {
        _monAnService = monAnService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public GioHangViewModel Get(ISession session)
    {
        var json = session.GetString(CartKey);
        var cart = string.IsNullOrWhiteSpace(json) 
            ? new List<GioHangItemViewModel>() 
            : JsonSerializer.Deserialize<List<GioHangItemViewModel>>(json) ?? new();

        foreach (var it in cart)
        {
            if (string.IsNullOrWhiteSpace(it.DuongDanAnh) || it.DuongDanAnh == "/images/default.jpg")
            {
                var d = _monAnService.Find(it.MaMon);
                if (d != null && !string.IsNullOrWhiteSpace(d.DuongDanAnh)) 
                    it.DuongDanAnh = d.DuongDanAnh;
            }
        }

        var voucher = session.GetString(VoucherKey);
        var subtotal = cart.Sum(x => x.ThanhTien);
        var discount = string.IsNullOrWhiteSpace(voucher) ? 0m : CalculateDiscountForCart(session, subtotal);

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
        if (item is null)
        {
            cart.Add(new GioHangItemViewModel 
            { 
                MaMon = dish.MaMon, 
                TenMon = dish.TenMon, 
                DuongDanAnh = dish.DuongDanAnh, 
                BieuTuong = dish.BieuTuong, 
                DonGia = (dish.DonGiaSauGiam > 0 && dish.DonGiaSauGiam < dish.DonGia) ? dish.DonGiaSauGiam : dish.DonGia, 
                SoLuong = Math.Clamp(quantity, 1, 99) 
            });
        }
        else
        {
            if (string.IsNullOrWhiteSpace(item.DuongDanAnh) || item.DuongDanAnh == "/images/default.jpg") 
                item.DuongDanAnh = dish.DuongDanAnh;
            item.SoLuong = Math.Min(99, item.SoLuong + Math.Clamp(quantity, 1, 99));
        }
        Save(session, cart);
    }

    public void SetQuantity(ISession session, string maMon, int quantity)
    {
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => x.MaMon == maMon);
        if (item is not null)
        {
            if (quantity <= 0) cart.Remove(item);
            else item.SoLuong = Math.Min(quantity, 99);
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

    public async Task<(bool Success, string Message, decimal Discount)> ApplyVoucherAsync(ISession session, string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            session.Remove(VoucherKey);
            session.Remove(VoucherInfoKey);
            return (true, "Đã gỡ mã ưu đãi khỏi đơn hàng.", 0m);
        }

        var clean = code.Trim().ToUpperInvariant();
        var cart = Get(session);
        var subtotal = cart.TamTinh;

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

                    session.SetString(VoucherKey, savedCode);

                    var voucherInfo = new AppliedVoucherSessionData
                    {
                        Code = savedCode,
                        MaKmvoucher = apiResult.MaKmvoucher,
                        TenChuongTrinh = apiResult.TenChuongTrinh,
                        PhanTramGiam = apiResult.PhanTramGiam,
                        TienGiam = apiResult.TienGiam,
                        GiaTriDonToiThieu = 0,
                        GiamToiDa = apiResult.GiamToiDa,
                        PhamViApDung = apiResult.PhamViApDung,
                        MaLoaiMon = apiResult.MaLoaiMon,
                        MaMon = apiResult.MaMon,
                        LastCalculatedDiscount = (decimal)apiResult.GiaTriGiamThucTe
                    };
                    session.SetString(VoucherInfoKey, JsonSerializer.Serialize(voucherInfo));

                    var msg = !string.IsNullOrWhiteSpace(apiResult.Message) 
                        ? apiResult.Message 
                        : $"Áp dụng voucher {savedCode} thành công! Giảm ngay {apiResult.GiaTriGiamThucTe:N0}đ.";

                    return (true, msg, (decimal)apiResult.GiaTriGiamThucTe);
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
        return (false, "Không thể kết nối đến hệ thống kiểm tra voucher. Vui lòng thử lại sau.", 0m);
    }

    public (bool Success, string Message, decimal Discount) ApplyVoucher(ISession session, string? code)
    {
        return ApplyVoucherAsync(session, code).GetAwaiter().GetResult();
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
                if (info != null)
                {
                    if (info.GiaTriDonToiThieu.HasValue && subtotal < (decimal)info.GiaTriDonToiThieu.Value)
                    {
                        return 0m;
                    }

                    if (info.PhanTramGiam.HasValue && info.PhanTramGiam.Value > 0)
                    {
                        var disc = Math.Round(subtotal * (decimal)info.PhanTramGiam.Value / 100m, 0);
                        if (info.GiamToiDa.HasValue && info.GiamToiDa.Value > 0 && disc > (decimal)info.GiamToiDa.Value)
                        {
                            disc = (decimal)info.GiamToiDa.Value;
                        }
                        return disc;
                    }
                    else if (info.TienGiam.HasValue && info.TienGiam.Value > 0)
                    {
                        return Math.Min(subtotal, (decimal)info.TienGiam.Value);
                    }
                }
            }
            catch { }
        }

        return 0m;
    }

    public static decimal CalculateDiscount(string code, decimal subtotal, List<GioHangItemViewModel>? items = null, MonAnService? monAnService = null)
    {
        // Legacy fallback
        return 0m;
    }

    public void Clear(ISession session)
    {
        session.Remove(CartKey);
        session.Remove(VoucherKey);
        session.Remove(VoucherInfoKey);
    }

    private static void Save(ISession session, List<GioHangItemViewModel> cart) => 
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
}
