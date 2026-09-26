using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.PhanHoi {
    public class PhanHoiForm : ModuleFormBase {
        public PhanHoiForm() : base("Phản hồi", "Tiếp nhận ý kiến khách hàng và theo dõi việc chăm sóc.",
            new[] { "Mã phản hồi", "Khách hàng", "Ngày gửi", "Đánh giá", "Nội dung", "Trạng thái" },
            new[] { new ModuleField("id", "Mã phản hồi", true), new ModuleField("customer", "Khách hàng", true), new ModuleField("date", "Ngày gửi", false), new ModuleField("rating", "Đánh giá /5", false), new ModuleField("content", "Nội dung", true), new ModuleField("status", "Trạng thái", false) },
            new[] { new[] { "PH001", "Nguyễn Minh Anh", "26/09/2026", "5", "Món ăn ngon, phục vụ nhanh.", "Mới" }, new[] { "PH002", "Trần Quốc Bảo", "25/09/2026", "3", "Thời gian chờ hơi lâu.", "Đang xử lý" } }, ThemeManager.Purple) { }
    }
}
