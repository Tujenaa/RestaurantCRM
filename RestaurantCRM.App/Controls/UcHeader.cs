using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcHeader : UserControl {
        public Label TitleLabel { get; set; }
        public UcHeader() {
            this.Height = 70;
            this.Dock = DockStyle.Top;
            this.BackColor = ThemeManager.Paper;
            
            TitleLabel = new Label {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(25, 20)
            };
            this.Controls.Add(TitleLabel);

            Label lblUser = new Label {
                Text = "AD - Quản trị viên",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(this.Width - 160, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            this.Controls.Add(lblUser);
        }
    }
}
