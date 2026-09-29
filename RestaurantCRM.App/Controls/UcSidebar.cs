using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Controls {
    public class UcSidebar : UserControl {
        public event Action<string> MenuSelected;
        public event Action<string> WorkspaceSelected;
        public UcSidebar(string role, IList<string> roles, string displayName) {
            Width = 252; Dock = DockStyle.Left; AutoScroll = true;
            BackColor = role == "CRM" ? Color.FromArgb(56, 35, 77) : role == "Bán hàng" ? Color.FromArgb(11, 77, 71) : Color.FromArgb(23, 61, 114);
            var brand = new Label { Text = "◆  RestaurantCRM\n    " + role, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold), AutoSize = true, Location = new Point(22, 24) };
            Controls.Add(brand);
            int top = 92;
            if (roles != null && roles.Count > 1) {
                Controls.Add(new Label { Text = "CHUYỂN PHÂN HỆ", ForeColor = Color.FromArgb(194, 212, 239), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(20, top) });
                top += 21;
                foreach (string permittedRole in roles) {
                    var switchButton = MakeButton("↔  " + permittedRole, top);
                    switchButton.Height = 31;
                    switchButton.BackColor = permittedRole == role ? Color.FromArgb(65, 255, 255, 255) : Color.Transparent;
                    switchButton.Click += (s, e) => WorkspaceSelected?.Invoke(permittedRole);
                    Controls.Add(switchButton); top += 34;
                }
                top += 11;
            }

            AddSection("TỔNG QUAN", new[] { "Dashboard" }, top); top += 61;
            if (role == "Admin") {
                AddSection("BÁN HÀNG & KHO", new[] { "Món ăn", "Nhà cung cấp", "Phiếu nhập hàng", "Khuyến mãi" }, top); top += 165;
                AddSection("HỆ THỐNG", new[] { "Tài khoản", "Vai trò & phân quyền" }, top);
            } else if (role == "Bán hàng") {
                AddSection("BÁN HÀNG & KHO", new[] { "Tạo đơn hàng", "Đơn hàng", "Thực đơn", "Món ăn", "Nhà cung cấp", "Phiếu nhập hàng", "Khuyến mãi", "Báo cáo bán hàng" }, top);
            } else {
                AddSection("KHÁCH HÀNG", new[] { "Khách hàng", "Đánh giá", "Phản hồi" }, top); top += 127;
                AddSection("KHẢO SÁT & BÁO CÁO", new[] { "Khảo sát", "Phân tích khách hàng" }, top);
            }

            var foot = new Panel { Dock = DockStyle.Bottom, Height = 82, Padding = new Padding(16, 8, 12, 8), BackColor = Color.Transparent };
            foot.Controls.Add(new Label { Text = displayName + "\n" + role, ForeColor = Color.FromArgb(220, 233, 255), AutoSize = true, Location = new Point(4, 3), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
            var logout = new Button { Text = "Đăng xuất", ForeColor = Color.White, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Bottom, Height = 34, Cursor = Cursors.Hand };
            logout.FlatAppearance.BorderSize = 0; logout.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 255, 255, 255); logout.Click += (s, e) => MenuSelected?.Invoke("Đăng xuất"); foot.Controls.Add(logout); Controls.Add(foot);
        }

        private Button MakeButton(string text, int y) {
            var button = new Button { Text = "  " + text, ForeColor = Color.FromArgb(235, 241, 255), BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft, Size = new Size(228, 32), Location = new Point(12, y), Cursor = Cursors.Hand, Font = new Font("Segoe UI", 9F) };
            button.FlatAppearance.BorderSize = 0; button.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 255, 255, 255); return button;
        }
        private void AddSection(string title, string[] items, int top) {
            Controls.Add(new Label { Text = title, ForeColor = Color.FromArgb(194, 212, 239), Font = new Font("Segoe UI", 8F, FontStyle.Bold), AutoSize = true, Location = new Point(20, top) });
            int y = top + 22;
            foreach (string item in items) {
                var button = MakeButton(item, y); button.Tag = item;
                button.Click += (s, e) => MenuSelected?.Invoke((string)((Button)s).Tag);
                Controls.Add(button); y += 35;
            }
        }
    }
}
