using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Helpers {
    public class TokenHelper {
        
    }

    public static class UserSession {
        public static string CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static string CurrentDisplayName { get; set; }
        public static List<string> CurrentRoles { get; set; } = new List<string>();
        public static HashSet<string> Permissions { get; set; } = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

        public static bool HasPermission(string code) {
            if (CurrentRoles != null && CurrentRoles.Exists(r => 
                string.Equals(r, "Admin", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, "Hệ thống", System.StringComparison.OrdinalIgnoreCase))) {
                return true;
            }
            if (Permissions == null || Permissions.Count == 0) return false;
            if (Permissions.Contains("*") || Permissions.Contains("ALL")) return true;
            return Permissions.Contains(code);
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
