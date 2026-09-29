using System;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp {
    static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            while (true) {
                var login = new Forms.Auth.LoginForm();
                if (login.ShowDialog() != DialogResult.OK) {
                    break;
                }
                var workspace = CreateWorkspace(login.Username, login.DisplayName, login.Roles);
                if (workspace == null) break;
                Application.Run(workspace);

                var dashboard = workspace as Forms.Dashboard.DashboardForm;
                if (dashboard == null || !dashboard.IsLoggedOut) {
                    break;
                }
            }
        }

        private static Form CreateWorkspace(string username, string displayName, System.Collections.Generic.List<string> roles) {
            if (roles == null) return null;
            if (roles.Contains("Admin")) return new Forms.Dashboard.AdminShellForm(username, displayName, roles);
            if (roles.Contains("Bán hàng")) return new Forms.Dashboard.BanHangKhoShellForm(username, displayName);
            if (roles.Contains("CRM")) return new Forms.Dashboard.CrmShellForm(username, displayName);
            return null;
        }
    }
}
