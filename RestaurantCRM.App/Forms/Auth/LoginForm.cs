using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms.Dashboard;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.Auth {
    public class LoginForm : Form {
        private readonly TextBox _username;
        private readonly TextBox _password;
        private readonly Label _error;
        
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public List<string> Roles { get; private set; }

        public LoginForm() {
            Text = "RestaurantCRM | Đăng nhập";
            ClientSize = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen; BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 10F); AutoScaleMode = AutoScaleMode.Font;

            var card = new Panel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = ThemeManager.Paper, Padding = new Padding(40) };
            card.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
            Controls.Add(card);
            void CenterCard() { card.Left = (ClientSize.Width - card.Width) / 2; card.Top = (ClientSize.Height - card.Height) / 2; }
            Resize += (s, e) => CenterCard();
            card.SizeChanged += (s, e) => CenterCard();

            card.Controls.Add(new Label { Text = "◆", Font = new Font("Segoe UI", 28F, FontStyle.Bold), ForeColor = ThemeManager.Blue, AutoSize = true, Location = new Point(188, 25) });
            card.Controls.Add(new Label { Text = "Đăng nhập hệ thống", Font = new Font("Segoe UI", 21F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(50, 88) });
            card.Controls.Add(new Label { Text = "Nhập tài khoản được cấp. Hệ thống sẽ mở\nphân hệ phù hợp với quyền của bạn.", Font = new Font("Segoe UI", 10F), ForeColor = ThemeManager.Muted, TextAlign = ContentAlignment.MiddleCenter, Size = new Size(360, 48), Location = new Point(40, 132) });

            card.Controls.Add(new Label { Text = "Tên đăng nhập", Location = new Point(40, 205), AutoSize = true, ForeColor = ThemeManager.Muted });
            _username = new TextBox { Location = new Point(40, 231), Width = 360, Height = 38, Font = new Font("Segoe UI", 11F) }; card.Controls.Add(_username);
            card.Controls.Add(new Label { Text = "Mật khẩu", Location = new Point(40, 287), AutoSize = true, ForeColor = ThemeManager.Muted });
            _password = new TextBox { Location = new Point(40, 313), Width = 360, Height = 38, Font = new Font("Segoe UI", 11F), UseSystemPasswordChar = true }; card.Controls.Add(_password);
            _error = new Label { Text = "", Location = new Point(40, 365), Size = new Size(360, 30), ForeColor = ThemeManager.Red, TextAlign = ContentAlignment.MiddleCenter, Visible = false }; card.Controls.Add(_error);
            var login = new Button { Text = "Đăng nhập", Location = new Point(40, 405), Size = new Size(360, 46), BackColor = ThemeManager.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            login.FlatAppearance.BorderSize = 0; login.Click += (s, e) => SubmitLogin(); card.Controls.Add(login);
            card.Controls.Add(new Label { Text = "Tài khoản demo: admin, le.thu, vo.hung, cskh\nMật khẩu demo: nhập bất kỳ", Location = new Point(40, 468), Size = new Size(360, 42), ForeColor = ThemeManager.Muted, TextAlign = ContentAlignment.MiddleCenter });
            AcceptButton = login;
        }

        private void SubmitLogin() {
            string username = _username.Text.Trim();
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(_password.Text)) { ShowError("Vui lòng nhập tên đăng nhập và mật khẩu."); return; }
            List<string> roles = GetRoles(username);
            if (roles == null) { ShowError("Tài khoản không tồn tại hoặc đã bị khóa."); return; }
            
            Username = username;
            DisplayName = GetDisplayName(username);
            Roles = roles;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowError(string message) { _error.Text = message; _error.Visible = true; }
        private static List<string> GetRoles(string username) {
            switch (username.ToLowerInvariant()) {
                case "admin": return new List<string> { "Admin", "Bán hàng", "CRM" };
                case "le.thu": return new List<string> { "Bán hàng" };
                case "vo.hung": return new List<string> { "Bán hàng" };
                case "cskh": return new List<string> { "CRM" };
                default: return null;
            }
        }
        private static string GetDisplayName(string username) {
            switch (username.ToLowerInvariant()) {
                case "admin": return "Trần An";
                case "le.thu": return "Lê Thu";
                case "vo.hung": return "Võ Hùng";
                case "cskh": return "Nhân viên CSKH";
                default: return username;
            }
        }
    }
}
