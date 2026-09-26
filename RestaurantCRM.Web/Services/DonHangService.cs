using RestaurantCRM.Web.Models;

namespace RestaurantCRM.Web.Services;

public class DonHangService
{
    private readonly List<DonHangViewModel> _orders = new()
    {
        new() { MaDonHang = "DH00231", NgayDat = DateTime.Today.AddHours(-2), TongThanhToan = 487000, TrangThai = "Đang giao", TrangThaiLoc = "active", BuocHienTai = 3, TienTrinh = Steps(), ChiTiet = new() { new() { MaMon = "MA001", TenMon = "Lẩu Thái chua cay", SoLuong = 1, DonGia = 129000 }, new() { MaMon = "MA004", TenMon = "Ba chỉ bò Mỹ", SoLuong = 2, DonGia = 159000 }, new() { MaMon = "MA006", TenMon = "Rổ rau thập cẩm", SoLuong = 1, DonGia = 45000 } } },
        new() { MaDonHang = "DH00198", NgayDat = DateTime.Today.AddDays(-6), TongThanhToan = 312000, TrangThai = "Hoàn tất", TrangThaiLoc = "done", BuocHienTai = 4, TienTrinh = Steps(), ChiTiet = new() { new() { MaMon = "MA002", TenMon = "Lẩu nấm chay", SoLuong = 1, DonGia = 109000 }, new() { MaMon = "MA005", TenMon = "Set hải sản tươi", SoLuong = 1, DonGia = 189000 } } },
        new() { MaDonHang = "DH00172", NgayDat = DateTime.Today.AddDays(-14), TongThanhToan = 159000, TrangThai = "Đã hủy", TrangThaiLoc = "cancel", BuocHienTai = 0, TienTrinh = Steps(), ChiTiet = new() { new() { MaMon = "MA004", TenMon = "Ba chỉ bò Mỹ", SoLuong = 1, DonGia = 159000 } } }
    };

    public IReadOnlyList<DonHangViewModel> GetForDemoCustomer(string state = "all") => _orders.Where(x => state == "all" || x.TrangThaiLoc == state).OrderByDescending(x => x.NgayDat).ToList();

    public string Create(DatHangViewModel customer, decimal total, IReadOnlyList<GioHangItemViewModel> items)
    {
        var id = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
        _orders.Insert(0, new DonHangViewModel { MaDonHang = id, NgayDat = DateTime.Now, TongThanhToan = total, TrangThai = "Đã xác nhận", TrangThaiLoc = "active", BuocHienTai = 2, TienTrinh = Steps(), ChiTiet = items.Select(x => new DonHangChiTietViewModel { MaMon = x.MaMon, TenMon = x.TenMon, SoLuong = x.SoLuong, DonGia = x.DonGia }).ToList() });
        return id;
    }

    public DonHangViewModel? Find(string id) => _orders.FirstOrDefault(x => x.MaDonHang == id);

    private static List<string> Steps() => new() { "Đã đặt", "Xác nhận", "Đang chuẩn bị", "Đang giao", "Hoàn tất" };
}
