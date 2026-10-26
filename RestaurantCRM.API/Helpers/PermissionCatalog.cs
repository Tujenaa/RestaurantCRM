using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantCRM.API.Helpers
{
    public static class PermissionCatalog
    {
        public static readonly List<(string Code, string Name, string Group, string Description)> AllPermissions = new()
        {
            // Nhóm Vận Hành
            ("ORDER_CREATE", "Tạo đơn hàng mới", "Vận Hành", "Cho phép mở màn hình Tạo đơn hàng và lập hóa đơn mới"),
            ("ORDER_VIEW", "Quản lý đơn hàng", "Vận Hành", "Xem danh sách và trạng thái hóa đơn / đơn hàng"),
            ("MENU_VIEW", "Thực đơn món ăn", "Vận Hành", "Xem và tra cứu thực đơn món ăn"),
            ("PROMO_VIEW", "Khuyến mãi", "Vận Hành", "Quản lý chương trình khuyến mãi và voucher"),
            ("INVENTORY_VIEW", "Kiểm tra kho", "Vận Hành", "Xem thông tin tồn kho thực phẩm"),
            ("IMPORT_VIEW", "Quản lý phiếu nhập", "Vận Hành", "Xem và tạo phiếu nhập nguyên vật liệu"),
            ("INGREDIENT_VIEW", "Nguyên vật liệu", "Vận Hành", "Quản lý danh mục nguyên vật liệu nhà hàng"),
            ("REPORT_VIEW", "Báo cáo doanh thu", "Vận Hành", "Xem báo cáo thống kê doanh thu bán hàng"),

            // Nhóm Khách hàng
            ("CRM_CUSTOMER", "Quản lý khách hàng", "Khách hàng", "Xem và quản lý hồ sơ khách hàng"),
            ("CRM_REVIEW", "Đánh giá dịch vụ", "Khách hàng", "Xem và phản hồi đánh giá món ăn / dịch vụ"),
            ("CRM_FEEDBACK", "Góp ý & Phản hồi", "Khách hàng", "Xử lý góp ý và khiếu nại của khách"),
            ("CRM_SURVEY", "Khảo sát ý kiến", "Khách hàng", "Thiết lập và quản lý các phiếu khảo sát"),
            ("CRM_ANALYTICS", "Phân tích khách hàng", "Khách hàng", "Xem biểu đồ và báo cáo phân tích tệp khách hàng"),

            // Nhóm Hệ thống
            ("ADMIN_ACCOUNT", "Quản lý Tài khoản", "Hệ thống", "Quản trị danh sách tài khoản nhân viên"),
            ("ADMIN_ROLE", "Vai trò & Phân quyền", "Hệ thống", "Cấu hình vai trò và gán quyền chi tiết"),
            ("ADMIN_BACKUP", "Sao lưu & Phục hồi CSDL", "Hệ thống", "Toàn quyền sao lưu, tải về và phục hồi cơ sở dữ liệu")
        };

        public static List<string> ParsePermissions(string? raw, string? roleName = null)
        {
            if (string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleName, "Hệ thống", StringComparison.OrdinalIgnoreCase))
            {
                return AllPermissions.Select(p => p.Code).ToList();
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                if (string.Equals(roleName, "sale", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "Bán hàng", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "Vận Hành", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "Vận hành", StringComparison.OrdinalIgnoreCase))
                {
                    return AllPermissions.Where(p => p.Group == "Vận Hành" || p.Group == "Bán hàng & Kho").Select(p => p.Code).ToList();
                }

                if (string.Equals(roleName, "crm", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "CRM", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "Khách hàng", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "khách hàng", StringComparison.OrdinalIgnoreCase))
                {
                    return AllPermissions.Where(p => p.Group == "Khách hàng" || p.Group == "CRM Khách hàng").Select(p => p.Code).ToList();
                }

                return new List<string>();
            }

            if (raw.Trim() == "*")
            {
                return AllPermissions.Select(p => p.Code).ToList();
            }

            return raw.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim().ToUpperInvariant())
                      .Distinct()
                      .ToList();
        }

        public static string FormatPermissions(IEnumerable<string>? permissions)
        {
            if (permissions == null) return string.Empty;
            return string.Join(",", permissions.Select(p => p.Trim().ToUpperInvariant()).Distinct());
        }
    }
}
