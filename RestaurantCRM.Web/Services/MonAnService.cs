using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class MonAnService
{
    private static readonly IReadOnlyList<MonAnViewModel> Menu = new List<MonAnViewModel>
    {
        new() { MaMon = "MA001", TenMon = "Lẩu Thái chua cay", LoaiMon = "Nước lẩu", MoTa = "Sả, riềng, lá chanh, vị chua cay đậm đà.", DonGia = 129000, BieuTuong = "🥘", Tone = "red", Tag = "Bán chạy" },
        new() { MaMon = "MA002", TenMon = "Lẩu nấm chay", LoaiMon = "Nước lẩu", MoTa = "Nước dùng nấm hầm rau củ thanh ngọt.", DonGia = 109000, BieuTuong = "🍄", Tone = "green", Tag = "Thanh đạm" },
        new() { MaMon = "MA003", TenMon = "Lẩu Tứ Xuyên", LoaiMon = "Nước lẩu", MoTa = "Tê cay đậm vị dành cho tín đồ ăn cay.", DonGia = 149000, BieuTuong = "🌶️", Tone = "red", Tag = "Cay nồng" },
        new() { MaMon = "MA004", TenMon = "Ba chỉ bò Mỹ", LoaiMon = "Thịt", MoTa = "Thịt bò thái lát mỏng, mềm thơm, 300g.", DonGia = 159000, BieuTuong = "🥩", Tone = "gold", Tag = "Được yêu thích" },
        new() { MaMon = "MA005", TenMon = "Set hải sản tươi", LoaiMon = "Hải sản", MoTa = "Tôm, mực, nghêu tươi ngon, 400g.", DonGia = 189000, BieuTuong = "🍤", Tag = "Tươi mỗi ngày" },
        new() { MaMon = "MA006", TenMon = "Rổ rau thập cẩm", LoaiMon = "Rau & nấm", MoTa = "Cải thảo, rau muống, tần ô theo mùa.", DonGia = 45000, BieuTuong = "🥬", Tone = "green" },
        new() { MaMon = "MA007", TenMon = "Đậu hũ phô mai", LoaiMon = "Món ăn kèm", MoTa = "Viên đậu hũ béo thơm, ăn kèm lẩu.", DonGia = 39000, BieuTuong = "🧀", Tone = "gold" },
        new() { MaMon = "MA008", TenMon = "Mì trứng", LoaiMon = "Món ăn kèm", MoTa = "Mì trứng dai ngon, dùng cùng nước lẩu.", DonGia = 25000, BieuTuong = "🍜", Tone = "gold" },
        new() { MaMon = "MA009", TenMon = "Nấm tổng hợp", LoaiMon = "Rau & nấm", MoTa = "Nấm kim châm, nấm đùi gà, nấm bào ngư.", DonGia = 55000, BieuTuong = "🍄", Tone = "green" },
        new() { MaMon = "MA010", TenMon = "Bạch tuộc", LoaiMon = "Hải sản", MoTa = "Bạch tuộc giòn ngọt, sơ chế sạch.", DonGia = 99000, BieuTuong = "🐙" },
        new() { MaMon = "MA011", TenMon = "Ba chỉ heo", LoaiMon = "Thịt", MoTa = "Ba chỉ heo tươi, thái lát vừa ăn.", DonGia = 89000, BieuTuong = "🥓", Tone = "red" },
        new() { MaMon = "MA012", TenMon = "Nước ngọt", LoaiMon = "Món ăn kèm", MoTa = "Nước giải khát dùng kèm bữa ăn.", DonGia = 18000, BieuTuong = "🥤" }
    };

    public IReadOnlyList<MonAnViewModel> GetAll() => Menu;
    public IReadOnlyList<string> GetCategories() => new[] { "Tất cả", "Nước lẩu", "Thịt", "Hải sản", "Rau & nấm", "Món ăn kèm" };
    public MonAnViewModel? Find(string id) => Menu.FirstOrDefault(x => x.MaMon == id);
}
