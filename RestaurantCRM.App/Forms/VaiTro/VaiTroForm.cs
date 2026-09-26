using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.VaiTro {
    public class VaiTroForm : ModuleFormBase {
        public VaiTroForm() : base("Vai trò & phân quyền", "Xem và quản lý vai trò, phạm vi quyền của nhân viên.",
            new[] { "Mã vai trò", "Tên vai trò", "Mô tả", "Quyền truy cập" },
            new[] { new ModuleField("id", "Mã vai trò", true), new ModuleField("name", "Tên vai trò", true), new ModuleField("description", "Mô tả", false), new ModuleField("permissions", "Quyền truy cập", false) },
            new[] { new[] { "VT001", "Quản trị viên", "Quản trị toàn hệ thống", "Tất cả quyền" }, new[] { "VT002", "CSKH", "Khách hàng, phản hồi, khảo sát", "Đọc/ghi CRM" }, new[] { "VT003", "Thu ngân", "Đơn hàng và thanh toán", "Bán hàng" } }, ThemeManager.Blue) { }
    }
}
