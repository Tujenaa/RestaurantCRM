using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Helpers {
    public class TokenHelper {
        
    }

    public static class UserSession {
        public static string CurrentUserId { get; set; }
        public static string CurrentUsername { get; set; }
        public static string CurrentDisplayName { get; set; }
        public static List<string> CurrentRoles { get; set; } = new List<string>();
    }
}
