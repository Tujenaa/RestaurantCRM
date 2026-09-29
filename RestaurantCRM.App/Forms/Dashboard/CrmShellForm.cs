using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public sealed class CrmShellForm : DashboardForm {
        public CrmShellForm(string username, string displayName)
            : base(username, displayName, new List<string> { "CRM" }) { }
    }
}
