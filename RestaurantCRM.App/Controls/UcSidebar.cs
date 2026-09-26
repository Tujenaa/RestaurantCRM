using System;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcSidebar : UserControl {
        public event Action<string> MenuSelected;
        private readonly Color _accent;
        private readonly string _role;

        public UcSidebar(string role) {
            _role = role; _accent = role == "CRM" ? Color.FromArgb(105, 65, 165) : role == "Bán hàng" ? Color.FromArgb(14, 124, 134) : Color.FromArgb(36, 87, 167);
            Width = 250; Dock = DockStyle.Left; BackColor = role == "CRM" ? Color.FromArgb(56, 35, 77) : role == "Bán hàng" ? Color.FromArgb(11, 77, 71) : Color.FromArgb(23, 61, 114);
            var brand = new Label { Text = (role == "CRM" ? "●  " : role == "Bán hàng" ? "▣  " : "◆  ") + role + ".WinForms\nRestaurantCRM", ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(16, 18) };
            Controls.Add(brand);
            AddSection("TỔNG QUAN", new[] { "Dashboard" }, 82);
            if (role == "Admin") {
                AddSection("BÁN HÀNG & KHO", new[] { "Món ăn", "Nhà cung cấp", "Phiếu nhập hàng", "Khuyến mãi" }, 158);
                AddSection("HỆ THỐNG", new[] { "Tài khoản", "Vai trò & phân quyền" }, 370);
            } else if (role == "Bán hàng") {
                AddSection("BÁN HÀNG", new[] { "Tạo đơn hàng", "Đơn hàng" }, 158);
                AddSection("TRA CỨU", new[] { "Thực đơn", "Khuyến mãi", "Báo cáo bán hàng" }, 278);
            } else {
                AddSection("KHÁCH HÀNG", new[] { "Khách hàng", "Đánh giá", "Phản hồi" }, 158);
                AddSection("KHẢO SÁT & BÁO CÁO", new[] { "Khảo sát", "Phân tích khách hàng" }, 338);
            }
            var foot = new Panel { Dock = DockStyle.Bottom, Height = 76, Padding = new Padding(12), BackColor = Color.Transparent };
            foot.Controls.Add(new Label { Text = role.ToUpperInvariant() + " WORKSPACE", ForeColor = Color.FromArgb(194, 212, 239), AutoSize = true, Location = new Point(2, 6), Font = new Font("Segoe UI", 8F, FontStyle.Bold) });
            var logout = new Button { Text = "Đăng xuất", ForeColor = Color.White, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Bottom, Height = 34, Cursor = Cursors.Hand };
            logout.FlatAppearance.BorderColor = Color.FromArgb(100, 255, 255, 255); logout.Click += (s, e) => MenuSelected?.Invoke("Đăng xuất"); foot.Controls.Add(logout); Controls.Add(foot);
        }

        private void AddSection(string title, string[] items, int top) {
            Controls.Add(new Label { Text = title, ForeColor = Color.FromArgb(194, 212, 239), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(18, top) });
            int y = top + 24;
            foreach (string item in items) {
                var button = new Button { Text = "  " + item, Tag = item, ForeColor = Color.FromArgb(224, 234, 255), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Size = new Size(226, 36), Location = new Point(12, y), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9F) };
                button.FlatAppearance.BorderSize = 0; button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 255, 255, 255);
                button.Click += (s, e) => MenuSelected?.Invoke((string)((Button)s).Tag); Controls.Add(button); y += 39;
            }
        }
    }
}
