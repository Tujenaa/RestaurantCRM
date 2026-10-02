using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms.Dashboard;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Auth {
    public class LoginForm : Form {
        private readonly TextBox _username;
        private readonly TextBox _password;
        private readonly Label _error;
        private readonly AuthService _authService;
        
        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public List<string> Roles { get; private set; }

        public LoginForm() {
            _authService = new AuthService();
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
            _error = new Label { Text = "", Location = new Point(40, 358), Size = new Size(360, 42), ForeColor = ThemeManager.Red, TextAlign = ContentAlignment.MiddleCenter, Visible = false }; card.Controls.Add(_error);
            var login = new Button { Text = "Đăng nhập", Location = new Point(40, 405), Size = new Size(360, 46), BackColor = ThemeManager.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Cursor = Cursors.Hand };
            login.FlatAppearance.BorderSize = 0; login.Click += async (s, e) => await SubmitLogin(); card.Controls.Add(login);
            card.Controls.Add(new Label { Text = "Tài khoản mẫu: admin / quanly01 / thungan01 / phucvu01\nMật khẩu: 123456", Location = new Point(40, 468), Size = new Size(360, 42), ForeColor = ThemeManager.Muted, TextAlign = ContentAlignment.MiddleCenter });
            AcceptButton = login;
        }

        private async System.Threading.Tasks.Task SubmitLogin() {
            string username = _username.Text.Trim();
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(_password.Text)) { ShowError("Vui lòng nhập tên đăng nhập và mật khẩu."); return; }
            
            var result = await _authService.LoginAsync(username, _password.Text);
            
            if (result == null || result.UserInfo == null) { 
                ShowError(_authService.LastError ?? "Tên đăng nhập hoặc mật khẩu không đúng."); 
                return; 
            }
            
            List<string> roles = new List<string>();
            if (String.Equals(result.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(result.Role, "admin", StringComparison.OrdinalIgnoreCase) ||
                String.Equals(result.Role, "Quản lý", StringComparison.OrdinalIgnoreCase)) {
                roles = new List<string> { "Admin", "Bán hàng", "CRM" };
            } else if (String.Equals(result.Role, "sale", StringComparison.OrdinalIgnoreCase) || String.Equals(result.Role, "Nhân viên Bán hàng", StringComparison.OrdinalIgnoreCase)) {
                roles = new List<string> { "Bán hàng" };
            } else if (String.Equals(result.Role, "crm", StringComparison.OrdinalIgnoreCase) || String.Equals(result.Role, "Nhân viên CSKH", StringComparison.OrdinalIgnoreCase)) {
                roles = new List<string> { "CRM" };
            } else {
                roles = new List<string> { result.Role };
            }

            Username = result.UserInfo.Username;
            DisplayName = result.UserInfo.HoTen;
            Roles = roles;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowError(string message) { _error.Text = message; _error.Visible = true; }
    }
}
