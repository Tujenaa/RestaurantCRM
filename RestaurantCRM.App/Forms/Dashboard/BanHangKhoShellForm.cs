using System.Collections.Generic;

namespace RestaurantCRM.AdminApp.Forms.Dashboard {
    public sealed class BanHangKhoShellForm : DashboardForm {
        public BanHangKhoShellForm(string username, string displayName)
            : base(username, displayName, new List<string> { "Bán hàng" }) { }
    }
}
