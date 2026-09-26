using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.DanhGia {
    public class DanhGiaForm : ModuleFormBase {
        public DanhGiaForm() : base("Đánh giá", "Theo dõi đánh giá món ăn và ý kiến của khách hàng.",
            new[] { "Mã đánh giá", "Khách hàng", "Món ăn", "Số sao", "Ngày đánh giá", "Nội dung" },
            new[] { new ModuleField("id", "Mã đánh giá", true), new ModuleField("customer", "Khách hàng", true), new ModuleField("dish", "Món ăn", true), new ModuleField("rating", "Số sao", true), new ModuleField("date", "Ngày đánh giá", false), new ModuleField("content", "Nội dung", false) },
            new[] { new[] { "DG001", "Nguyễn Minh Anh", "Phở bò", "5", "26/09/2026", "Nước dùng đậm đà." }, new[] { "DG002", "Trần Quốc Bảo", "Gỏi cuốn", "4", "25/09/2026", "Tươi ngon, đóng gói tốt." } }, ThemeManager.Purple) { }
    }
}
