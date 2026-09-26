using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class PhanHoiService
{
    private readonly List<PhanHoiItemViewModel> _items = new()
    {
        new() { TenMon = "Lẩu Tứ Xuyên", DanhGia = 5, NoiDung = "Vị tê cay rất chuẩn, nước lẩu đậm đà.", NgayGui = DateTime.Today.AddDays(-4) },
        new() { TenMon = "Set hải sản tươi", DanhGia = 3, NoiDung = "Tôm tươi nhưng giao hơi trễ so với dự kiến.", NgayGui = DateTime.Today.AddDays(-8) }
    };

    public IReadOnlyList<PhanHoiItemViewModel> GetRecent() => _items.OrderByDescending(x => x.NgayGui).ToList();
    public void Add(string dishName, int rating, string content) => _items.Insert(0, new PhanHoiItemViewModel { TenMon = dishName, DanhGia = rating, NoiDung = content, NgayGui = DateTime.Now });
}
