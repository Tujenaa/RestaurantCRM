using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.KhaoSat {
    public class KhaoSatForm : ModuleFormBase {
        public KhaoSatForm() : base("Khảo sát", "Tạo và theo dõi khảo sát, nhóm khách hàng và số lượt phản hồi.",
            new[] { "Mã khảo sát", "Tiêu đề", "Số câu hỏi", "Đối tượng", "Phản hồi", "Trạng thái" },
            new[] { new ModuleField("id", "Mã khảo sát", true), new ModuleField("title", "Tiêu đề", true), new ModuleField("questions", "Số câu hỏi", true), new ModuleField("target", "Đối tượng", false), new ModuleField("responses", "Số phản hồi", false), new ModuleField("status", "Trạng thái", false) },
            new[] { new[] { "KS001", "Mức độ hài lòng", "5", "Khách hàng đã mua", "38", "Đang mở" }, new[] { "KS002", "Thực đơn mùa hè", "4", "Tất cả khách hàng", "72", "Đã đóng" } }, ThemeManager.Purple) { }
    }
}
