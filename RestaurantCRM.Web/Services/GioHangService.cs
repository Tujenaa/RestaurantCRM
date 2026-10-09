using System.Text.Json;
using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class GioHangService
{
    private const string CartKey = "customer-cart-v1";
    private const string VoucherKey = "customer-voucher-v1";
    private readonly MonAnService _monAnService;

    public GioHangService(MonAnService monAnService) => _monAnService = monAnService;

    public GioHangViewModel Get(ISession session)
    {
        var json = session.GetString(CartKey);
        var cart = String.IsNullOrWhiteSpace(json) ? new List<GioHangItemViewModel>() : JsonSerializer.Deserialize<List<GioHangItemViewModel>>(json) ?? new();
        var voucher = session.GetString(VoucherKey);
        var subtotal = cart.Sum(x => x.ThanhTien);
        var discount = string.IsNullOrWhiteSpace(voucher) ? 0m : CalculateDiscount(voucher, subtotal);
        return new GioHangViewModel { Items = cart, MaVoucher = voucher, GiamGia = discount };
    }

    public void Add(ISession session, string maMon, int quantity)
    {
        var dish = _monAnService.Find(maMon);
        if (dish is null) return;
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => x.MaMon == maMon);
        if (item is null) cart.Add(new GioHangItemViewModel { MaMon = dish.MaMon, TenMon = dish.TenMon, BieuTuong = dish.BieuTuong, DonGia = (dish.DonGiaSauGiam > 0 && dish.DonGiaSauGiam < dish.DonGia) ? dish.DonGiaSauGiam : dish.DonGia, SoLuong = Math.Clamp(quantity, 1, 99) });
        else item.SoLuong = Math.Min(99, item.SoLuong + Math.Clamp(quantity, 1, 99));
        Save(session, cart);
        AutoApplyBestVoucher(session, cart);
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
        AutoApplyBestVoucher(session, cart);
    }

    public (bool Success, string Message, decimal Discount) ApplyVoucher(ISession session, string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            session.Remove(VoucherKey);
            return (true, "Đã gỡ mã ưu đãi khỏi đơn hàng.", 0m);
        }

        var clean = code.Trim().ToUpperInvariant();
        var cart = Get(session);
        var subtotal = cart.TamTinh;

        if (cart.Items.Count == 0)
        {
            session.SetString(VoucherKey, clean);
            return (true, $"Đã lưu mã {clean}. Hãy chọn thêm món ăn vào giỏ để nhận ưu đãi!", 0m);
        }

        decimal discount = CalculateDiscount(clean, subtotal);
        if (discount > 0)
        {
            session.SetString(VoucherKey, clean);
            return (true, $"Áp dụng voucher {clean} thành công! Giảm ngay {discount:N0}đ.", discount);
        }

        // Nếu mã hợp lệ nhưng chưa đạt giá trị tối thiểu
        if (clean == "NHOM15" || clean == "KM05" || clean == "KMV03")
        {
            return (false, $"Mã {clean} (giảm 15%) áp dụng cho đơn từ 500.000đ trở lên (hiện tại: {subtotal:N0}đ).", 0m);
        }

        if (clean == "VOUCHER40K" || clean == "KM02" || clean == "KMV02")
        {
            return (false, $"Mã {clean} (giảm 40.000đ) áp dụng cho đơn từ 200.000đ trở lên (hiện tại: {subtotal:N0}đ).", 0m);
        }

        session.Remove(VoucherKey);
        return (false, "Mã ưu đãi không hợp lệ. Các mã khả dụng: LAUPHO20 (-20k), VOUCHER40K (-40k), VOUCHER10 (-10%), NHOM15 (-15%).", 0m);
    }

    public static decimal CalculateDiscount(string code, decimal subtotal)
    {
        var clean = code.Trim().ToUpperInvariant();
        return clean switch
        {
            "LAUPHO20" or "KMV04" => Math.Min(subtotal, 20000m),
            "VOUCHER40K" or "KM02" or "KMV02" => subtotal >= 200000m ? 40000m : 0m,
            "VOUCHER10" or "KM01" or "KMV01" => Math.Round(subtotal * 0.10m, 0),
            "NHOM15" or "KM05" or "KMV03" => subtotal >= 500000m ? Math.Round(subtotal * 0.15m, 0) : 0m,
            _ => 0m
        };
    }

    public void Clear(ISession session)
    {
        session.Remove(CartKey);
        session.Remove(VoucherKey);
    }

    private static void Save(ISession session, List<GioHangItemViewModel> cart) => session.SetString(CartKey, JsonSerializer.Serialize(cart));

    private void AutoApplyBestVoucher(ISession session, List<GioHangItemViewModel> cart)
    {
        var subtotal = cart.Sum(x => x.ThanhTien);
        if (subtotal == 0) return;
        
        var currentVoucher = session.GetString(VoucherKey);
        var currentDiscount = string.IsNullOrWhiteSpace(currentVoucher) ? 0m : CalculateDiscount(currentVoucher, subtotal);

        var allVouchers = new[] { "LAUPHO20", "VOUCHER40K", "VOUCHER10", "NHOM15" };
        var bestVoucher = currentVoucher;
        var maxDiscount = currentDiscount;

        foreach (var v in allVouchers)
        {
            var d = CalculateDiscount(v, subtotal);
            if (d > maxDiscount)
            {
                maxDiscount = d;
                bestVoucher = v;
            }
        }

        if (bestVoucher != currentVoucher && maxDiscount > 0)
        {
            session.SetString(VoucherKey, bestVoucher);
        }
    }
}
