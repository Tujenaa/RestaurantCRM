using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.KhuyenMai {
    public class KhuyenMaiForm : ModuleFormBase {
        public KhuyenMaiForm() : base("Khuyến mãi", "Quản lý chương trình khuyến mãi và thời gian áp dụng.",
            new[] { "Mã chương trình", "Tên chương trình", "Loại", "Giá trị", "Bắt đầu", "Kết thúc", "Trạng thái" },
            new[] { new ModuleField("id", "Mã chương trình", true), new ModuleField("name", "Tên chương trình", true), new ModuleField("type", "Loại khuyến mãi", false), new ModuleField("value", "Giá trị giảm", false), new ModuleField("start", "Ngày bắt đầu", false), new ModuleField("end", "Ngày kết thúc", false), new ModuleField("status", "Trạng thái", false) },
            new[] { new[] { "KM001", "Ưu đãi khai trương", "Phần trăm", "10%", "01/09/2026", "30/09/2026", "Đang áp dụng" }, new[] { "KM002", "Combo gia đình", "Giảm tiền", "50000", "15/09/2026", "15/10/2026", "Đang áp dụng" } }, ThemeManager.Blue) { }
    }
}
