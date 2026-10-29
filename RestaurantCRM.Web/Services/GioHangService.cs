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
        return (false, "Không thể kết nối đến hệ thống kiểm tra voucher. Vui lòng thử lại sau.", 0m);
    }

    public (bool Success, string Message, decimal Discount) ApplyVoucher(ISession session, string? code)
    {
        return ApplyVoucherAsync(session, code).GetAwaiter().GetResult();
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

    public void Clear(ISession session)
    {
        session.Remove(CartKey);
        session.Remove(VoucherKey);
        session.Remove(VoucherInfoKey);
    }

    private static void Save(ISession session, List<GioHangItemViewModel> cart) => 
        session.SetString(CartKey, JsonSerializer.Serialize(cart));
}
