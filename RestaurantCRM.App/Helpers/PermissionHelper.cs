namespace RestaurantCRM.AdminApp.Helpers {
    public static class PermissionHelper {
        public static bool CanOpen(string role, string module) {
            if (role == "Admin")
                return module == "Đơn hàng" || module == "Hóa đơn" || module == "Món ăn" || module == "Loại món" || module == "Kho hàng" || module == "Tồn kho" || module == "Nhà cung cấp" || module == "Phiếu nhập hàng" || module == "Phiếu nhập" || module == "Khuyến mãi" || module == "Tài khoản" || module == "Vai trò & phân quyền";
            if (role == "Bán hàng")
                return module == "Tạo đơn hàng" || module == "Đơn hàng" || module == "Thực đơn" || module == "Món ăn" || module == "Loại món" || module == "Kho hàng" || module == "Tồn kho" || module == "Nhà cung cấp" || module == "Phiếu nhập hàng" || module == "Phiếu nhập" || module == "Khuyến mãi" || module == "Báo cáo bán hàng";
            if (role == "CRM")
                return module == "Khách hàng" || module == "Đánh giá" || module == "Phản hồi" || module == "Khảo sát" || module == "Phân tích khách hàng";
            return false;
        }
    }
}
