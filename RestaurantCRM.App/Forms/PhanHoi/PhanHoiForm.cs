using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.PhanHoi {
    public partial class PhanHoiForm : Form {
        public PhanHoiForm() {
            this.Text = "Quản lý PhanHoi";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = ThemeManager.Bg;
            this.Font = new Font("Segoe UI", 10F);

            Label title = new Label { Text = "Quản lý PhanHoi", Font = new Font("Segoe UI", 18F, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            this.Controls.Add(title);

            Button btnAdd = new Button { Text = "＋ Thêm mới", BackColor = ThemeManager.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(120, 35), Location = new Point(840, 20) };
            btnAdd.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnAdd);

            DataGridView dgv = new DataGridView {
                Location = new Point(20, 80),
                Size = new Size(940, 450),
                BackgroundColor = ThemeManager.Paper,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgv);
        }
    }
}
