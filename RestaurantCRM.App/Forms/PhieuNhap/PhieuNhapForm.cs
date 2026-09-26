using RestaurantCRM.AdminApp.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.PhieuNhap {
    public class PhieuNhapForm : ModuleFormBase {
        public PhieuNhapForm() : base("Phiếu nhập hàng", "Ghi nhận nhập kho theo nhà cung cấp và nhân viên phụ trách.",
            new[] { "Mã phiếu", "Nhà cung cấp", "Ngày nhập", "Nhân viên", "Tổng tiền", "Ghi chú" },
            new[] { new ModuleField("id", "Mã phiếu nhập", true), new ModuleField("supplier", "Nhà cung cấp", true), new ModuleField("date", "Ngày nhập", true), new ModuleField("staff", "Nhân viên", false), new ModuleField("total", "Tổng tiền", false), new ModuleField("note", "Ghi chú", false) },
            new[] { new[] { "PN001", "Thực phẩm An Toàn", "25/09/2026", "Lê Thu", "2450000", "Nguyên liệu tuần 39" }, new[] { "PN002", "Nông sản Xanh", "23/09/2026", "Trần An", "1780000", "Rau củ tươi" } }, ThemeManager.Blue) { }
    }
}
