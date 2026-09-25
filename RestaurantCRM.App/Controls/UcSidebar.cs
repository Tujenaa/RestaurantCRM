using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcSidebar : UserControl {
        public UcSidebar() {
            this.Width = 250;
            this.Dock = DockStyle.Left;
            this.BackColor = Color.FromArgb(23, 61, 114);

            Label lblBrand = new Label {
                Text = "🛡️ Admin.WinForms\nQuản trị hệ thống",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 15)
            };
            this.Controls.Add(lblBrand);
            
            int y = 80;
            string[] menus = {"Dashboard", "Tài khoản", "Phân quyền", "Món ăn", "Khách hàng"};
            foreach(var m in menus) {
                Button btn = new Button {
                    Text = "  " + m,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Size = new Size(220, 40),
                    Location = new Point(15, y),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                this.Controls.Add(btn);
                y += 45;
            }
        }
    }
}
