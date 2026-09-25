using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcThongKe : UserControl {
        public UcThongKe(string label, string value, string sub) {
            this.Size = new Size(200, 100);
            this.BackColor = ThemeManager.Paper;
            this.BorderStyle = BorderStyle.FixedSingle;

            Label lblName = new Label { Text = label, ForeColor = ThemeManager.Muted, Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 9F) };
            Label lblValue = new Label { Text = value, ForeColor = ThemeManager.Ink, Location = new Point(15, 35), AutoSize = true, Font = new Font("Segoe UI", 20F, FontStyle.Bold) };
            Label lblSub = new Label { Text = sub, ForeColor = ThemeManager.Muted, Location = new Point(15, 75), AutoSize = true, Font = new Font("Segoe UI", 8F) };

            this.Controls.Add(lblName);
            this.Controls.Add(lblValue);
            this.Controls.Add(lblSub);
        }
    }
}
