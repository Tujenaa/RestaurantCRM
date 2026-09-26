using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.KhachHang {
    public class KhachHangForm : ModuleFormBase {
        public KhachHangForm() : base("Khách hàng", "Hồ sơ khách hàng CRM và thông tin liên hệ.",
            new[] { "Mã KH", "Họ tên", "Điện thoại", "Email", "Ngày sinh", "Trạng thái" },
            new[] { new ModuleField("id", "Mã khách hàng", true), new ModuleField("name", "Họ tên", true), new ModuleField("phone", "Số điện thoại", true), new ModuleField("email", "Email", false), new ModuleField("birthday", "Ngày sinh", false), new ModuleField("status", "Trạng thái", false) },
            new[] { new[] { "KH001", "Nguyễn Minh Anh", "0901234567", "anh@example.vn", "12/05/1995", "Hoạt động" }, new[] { "KH002", "Trần Quốc Bảo", "0912345678", "bao@example.vn", "03/11/1990", "Hoạt động" }, new[] { "KH003", "Lê Thu Hà", "0987654321", "ha@example.vn", "21/08/1998", "Mới" } }, ThemeManager.Purple) { }
    }
}
