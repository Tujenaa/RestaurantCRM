using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.DonHang {
    public class DonHangForm : ModuleFormBase {
        public DonHangForm() : base("Đơn hàng", "Theo dõi đơn hàng, khách hàng, thanh toán và trạng thái xử lý.",
            new[] { "Mã đơn", "Khách hàng", "Ngày đặt", "Tổng tiền", "Thanh toán", "Trạng thái" },
            new[] { new ModuleField("id", "Mã đơn hàng", true), new ModuleField("customer", "Khách hàng", true), new ModuleField("date", "Ngày đặt", true), new ModuleField("total", "Tổng tiền", true), new ModuleField("payment", "Phương thức thanh toán", false), new ModuleField("status", "Trạng thái", false) },
            new[] { new[] { "DH001", "Nguyễn Minh Anh", "26/09/2026", "145000", "Tiền mặt", "Chờ xác nhận" }, new[] { "DH002", "Trần Quốc Bảo", "26/09/2026", "210000", "Chuyển khoản", "Đang chuẩn bị" }, new[] { "DH003", "Lê Thu Hà", "25/09/2026", "95000", "Tiền mặt", "Hoàn tất" } }, ThemeManager.Blue) { }
    }
}
