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
        var discount = subtotal >= 300000m ? 20000m : 0m;
        return new GioHangViewModel { Items = cart, MaVoucher = voucher, GiamGia = discount };
    }

    public void Add(ISession session, string maMon, int quantity)
    {
        var dish = _monAnService.Find(maMon);
        if (dish is null) return;
        var cart = Get(session).Items;
        var item = cart.FirstOrDefault(x => x.MaMon == maMon);
        if (item is null) cart.Add(new GioHangItemViewModel { MaMon = dish.MaMon, TenMon = dish.TenMon, BieuTuong = dish.BieuTuong, DonGia = dish.DonGia, SoLuong = Math.Clamp(quantity, 1, 99) });
        else item.SoLuong = Math.Min(99, item.SoLuong + Math.Clamp(quantity, 1, 99));
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

    public bool ApplyVoucher(ISession session, string code)
    {
        if (String.Equals(code.Trim(), "LAUPHO20", StringComparison.OrdinalIgnoreCase))
        {
            session.SetString(VoucherKey, "LAUPHO20");
            return true;
        }
        session.Remove(VoucherKey);
        return false;
    }

    public void Clear(ISession session)
    {
        session.Remove(CartKey);
        session.Remove(VoucherKey);
    }

    private static void Save(ISession session, List<GioHangItemViewModel> cart) => session.SetString(CartKey, JsonSerializer.Serialize(cart));
}
