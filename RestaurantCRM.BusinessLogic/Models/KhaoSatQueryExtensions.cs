using System.Linq;

namespace RestaurantCRM.BusinessLogic.Models;

public static class KhaoSatQueryExtensions
{
    /// <summary>Chỉ lấy khảo sát thường, bỏ bộ "Hồ sơ sở thích" (laSoThich = 1).</summary>
    public static IQueryable<KhaoSat> ChiKhaoSatThuong(this IQueryable<KhaoSat> query) =>
        query.Where(k => !k.LaSoThich);
}
