namespace RestaurantCRM.AdminApp.Helpers {
    public static class PermissionHelper {
        public static bool CanOpen(string role, string module) {
            if (role == "Admin" || UserSession.CurrentRoles.Contains("Admin")) {
                if (module == "Tài khoản") return UserSession.HasPermission("ADMIN_ACCOUNT");
                if (module == "Vai trò & phân quyền") return UserSession.HasPermission("ADMIN_ROLE");
                if (module == "Sao lưu dữ liệu") return UserSession.HasPermission("ADMIN_BACKUP");
            }

            switch (module) {
                case "Tài khoản": return UserSession.HasPermission("ADMIN_ACCOUNT");
                case "Vai trò & phân quyền": return UserSession.HasPermission("ADMIN_ROLE");
                case "Sao lưu dữ liệu": return UserSession.HasPermission("ADMIN_BACKUP");
                case "Tạo đơn hàng": return UserSession.HasPermission("ORDER_CREATE");
                case "Đơn hàng": case "Hóa đơn": return UserSession.HasPermission("ORDER_VIEW");
                case "Thực đơn": case "Món ăn": case "Loại món": return UserSession.HasPermission("MENU_VIEW");
                case "Khuyến mãi": return UserSession.HasPermission("PROMO_VIEW");
                case "Kho hàng": case "Tồn kho": return UserSession.HasPermission("INVENTORY_VIEW");
                case "Phiếu nhập hàng": case "Phiếu nhập": return UserSession.HasPermission("IMPORT_VIEW");
                case "Nguyên vật liệu": case "Nguyên liệu": case "Nhà cung cấp": return UserSession.HasPermission("INGREDIENT_VIEW");
                case "Báo cáo bán hàng": return UserSession.HasPermission("REPORT_VIEW");
                case "Khách hàng": return UserSession.HasPermission("CRM_CUSTOMER");
                case "Đánh giá": return UserSession.HasPermission("CRM_REVIEW");
                case "Phản hồi": return UserSession.HasPermission("CRM_FEEDBACK");
                case "Khảo sát": return UserSession.HasPermission("CRM_SURVEY");
                case "Phân tích khách hàng": return UserSession.HasPermission("CRM_ANALYTICS");
                default: return true;
            }
        }
    }
}
