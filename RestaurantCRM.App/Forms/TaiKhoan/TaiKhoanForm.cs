using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.TaiKhoan {
    public class TaiKhoanForm : ModuleFormBase {
        public TaiKhoanForm() : base("Tài khoản", "Quản lý tài khoản và phân hệ được cấp.",
            new[] { "Mã NV", "Họ tên", "Tên đăng nhập", "Phân hệ", "Trạng thái" },
            new[] {
                new ModuleField("id", "Mã nhân viên", true),
                new ModuleField("name", "Họ tên", true),
                new ModuleField("username", "Tên đăng nhập", true),
                new ModuleField("role", "Phân hệ", true, new[] { "Admin", "Bán hàng", "CRM" }),
                new ModuleField("status", "Trạng thái", true, new[] { "Hoạt động", "Tạm khóa" })
            },
            new[] {
                new[] { "NV001", "Trần An", "admin", "Admin", "Hoạt động" },
                new[] { "NV002", "Lê Thu", "le.thu", "Bán hàng", "Hoạt động" },
                new[] { "NV003", "Võ Hùng", "vo.hung", "Bán hàng", "Hoạt động" },
                new[] { "NV004", "Nhân viên CSKH", "cskh", "CRM", "Hoạt động" }
            }, ThemeManager.Blue) { }
    }
}
