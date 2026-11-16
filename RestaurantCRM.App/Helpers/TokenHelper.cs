using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Helpers {
    public static class UserSession {
        public static string CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static string CurrentDisplayName { get; set; }
        public static List<string> CurrentRoles { get; set; } = new List<string>();
        public static HashSet<string> Permissions { get; set; } = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        // Chỉ xét quyền thật được cấp trong VAI_TRO_QUYEN (không ngoại lệ theo vai trò)
        public static bool HasPermission(string code) {
            return Permissions != null && Permissions.Contains(code);
        }

        public static bool HasAnyPermission(params string[] codes) {
            if (codes == null || codes.Length == 0) return true;
            foreach (var code in codes) {
                if (HasPermission(code)) return true;
            }
            return false;
        }
    }
}
