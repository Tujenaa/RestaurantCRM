using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.TaiKhoan {
    public class TaiKhoanForm : ModuleFormBase {
        public TaiKhoanForm() : base("Tài khoản", "Quản lý tài khoản nhân viên và trạng thái truy cập.",
            new[] { "Mã NV", "Họ tên", "Tên đăng nhập", "Vai trò", "Trạng thái" },
            new[] { new ModuleField("id", "Mã nhân viên", true), new ModuleField("name", "Họ tên", true), new ModuleField("username", "Tên đăng nhập", true), new ModuleField("role", "Vai trò", true), new ModuleField("status", "Trạng thái", false) },
            new[] { new[] { "NV001", "Trần An", "admin", "Quản trị viên", "Hoạt động" }, new[] { "NV002", "Lê Thu", "le.thu", "Thu ngân", "Hoạt động" }, new[] { "NV003", "Đặng Quốc Huy", "huy.dq", "Giao hàng", "Đã khóa" } }, ThemeManager.Blue) { }
    }
}
