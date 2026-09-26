using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.NhaCungCap {
    public class NhaCungCapForm : ModuleFormBase {
        public NhaCungCapForm() : base("Nhà cung cấp", "Quản lý đối tác cung ứng và thông tin liên hệ.",
            new[] { "Mã NCC", "Tên nhà cung cấp", "Điện thoại", "Email", "Địa chỉ" },
            new[] { new ModuleField("id", "Mã nhà cung cấp", true), new ModuleField("name", "Tên nhà cung cấp", true), new ModuleField("phone", "Số điện thoại", false), new ModuleField("email", "Email", false), new ModuleField("address", "Địa chỉ", false) },
            new[] { new[] { "NCC001", "Thực phẩm An Toàn", "02838123456", "lienhe@antoan.vn", "Quận 3, TP. Hồ Chí Minh" }, new[] { "NCC002", "Nông sản Xanh", "02838765432", "kinhdoanh@nongsan.vn", "Thủ Đức, TP. Hồ Chí Minh" } }, ThemeManager.Blue) { }
    }
}
