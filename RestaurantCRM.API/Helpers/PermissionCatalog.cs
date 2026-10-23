using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantCRM.API.Helpers
{
    public static class PermissionCatalog
    {
        public static readonly List<(string Code, string Name, string Group, string Description)> AllPermissions = new()
        {
            // Nhóm Bán hàng & Kho
            ("ORDER_CREATE", "Tạo đơn hàng mới", "Bán hàng & Kho", "Cho phép mở màn hình Tạo đơn hàng và lập hóa đơn mới"),
            ("ORDER_VIEW", "Quản lý đơn hàng", "Bán hàng & Kho", "Xem danh sách và trạng thái hóa đơn / đơn hàng"),
            ("MENU_VIEW", "Thực đơn món ăn", "Bán hàng & Kho", "Xem và tra cứu thực đơn món ăn"),
            ("PROMO_VIEW", "Khuyến mãi", "Bán hàng & Kho", "Quản lý chương trình khuyến mãi và voucher"),
            ("INVENTORY_VIEW", "Kiểm tra kho", "Bán hàng & Kho", "Xem thông tin tồn kho thực phẩm"),
            ("IMPORT_VIEW", "Quản lý phiếu nhập", "Bán hàng & Kho", "Xem và tạo phiếu nhập nguyên vật liệu"),
            ("INGREDIENT_VIEW", "Nguyên vật liệu", "Bán hàng & Kho", "Quản lý danh mục nguyên vật liệu nhà hàng"),
            ("REPORT_VIEW", "Báo cáo doanh thu", "Bán hàng & Kho", "Xem báo cáo thống kê doanh thu bán hàng"),

            // Nhóm CRM Khách hàng
            ("CRM_CUSTOMER", "Quản lý khách hàng", "CRM Khách hàng", "Xem và quản lý hồ sơ khách hàng"),
            ("CRM_REVIEW", "Đánh giá dịch vụ", "CRM Khách hàng", "Xem và phản hồi đánh giá món ăn / dịch vụ"),
            ("CRM_FEEDBACK", "Góp ý & Phản hồi", "CRM Khách hàng", "Xử lý góp ý và khiếu nại của khách"),
            ("CRM_SURVEY", "Khảo sát ý kiến", "CRM Khách hàng", "Thiết lập và quản lý các phiếu khảo sát"),
            ("CRM_ANALYTICS", "Phân tích khách hàng", "CRM Khách hàng", "Xem biểu đồ và báo cáo phân tích tệp khách hàng"),

            // Nhóm Hệ thống (Admin)
            ("ADMIN_ACCOUNT", "Quản lý Tài khoản", "Hệ thống", "Quản trị danh sách tài khoản nhân viên"),
            ("ADMIN_ROLE", "Vai trò & Phân quyền", "Hệ thống", "Cấu hình vai trò và gán quyền chi tiết"),
            ("ADMIN_BACKUP", "Sao lưu & Phục hồi CSDL", "Hệ thống", "Toàn quyền sao lưu, tải về và phục hồi cơ sở dữ liệu")
        };

        public static List<string> ParsePermissions(string? raw, string? roleName = null)
        {
            if (string.Equals(roleName, "admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleName, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return AllPermissions.Select(p => p.Code).ToList();
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                if (string.Equals(roleName, "sale", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "Bán hàng", StringComparison.OrdinalIgnoreCase))
                {
                    return AllPermissions.Where(p => p.Group == "Bán hàng & Kho").Select(p => p.Code).ToList();
                }

                if (string.Equals(roleName, "crm", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(roleName, "CRM", StringComparison.OrdinalIgnoreCase))
                {
                    return AllPermissions.Where(p => p.Group == "CRM Khách hàng").Select(p => p.Code).ToList();
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
