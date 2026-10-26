using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public sealed class AdminShellForm : DashboardForm {
        public AdminShellForm(string username, string displayName, List<string> roles = null)
            : base(username, displayName, roles != null && roles.Count > 0 ? roles : new List<string> { "Hệ thống" }, "Hệ thống") { }
    }
}

