using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public sealed class AdminShellForm : DashboardForm {
        public AdminShellForm(string username, string displayName, List<string> roles)
            : base(username, displayName, roles) { }
    }
}
