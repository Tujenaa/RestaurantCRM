using System.Drawing;
using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.MonAn {
    public class MonAnForm : ModuleFormBase {
        public MonAnForm() : base("Món ăn", "Quản lý thực đơn, giá bán và tình trạng món.",
            new[] { "Mã món", "Ảnh minh họa", "Tên món", "Loại món", "Đơn giá", "Tồn kho", "Trạng thái" },
            new[] { new ModuleField("id", "Mã món", true), new ModuleField("image", "Ảnh minh họa", false, null, true), new ModuleField("name", "Tên món", true), new ModuleField("category", "Loại món", true, new[] { "Món chính", "Món phụ", "Khai vị", "Đồ uống", "Tráng miệng" }), new ModuleField("price", "Đơn giá", true), new ModuleField("quantity", "Số lượng", false), new ModuleField("status", "Trạng thái", false, new[] { "Đang bán", "Hết hàng", "Ngừng kinh doanh" }) },
            new[] { new[] { "MA001", "", "Phở bò", "Món chính", "65000", "24", "Đang bán" }, new[] { "MA002", "", "Gỏi cuốn", "Khai vị", "45000", "18", "Đang bán" }, new[] { "MA003", "", "Trà đào", "Đồ uống", "35000", "0", "Hết hàng" } }, ThemeManager.Blue) { }
    }
}
