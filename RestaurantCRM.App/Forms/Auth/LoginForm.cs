using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
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
            Text = "RestaurantCRM  |  Đăng nhập hệ thống";
            ClientSize = new Size(880, 660);
            MinimumSize = new Size(720, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);

            // Center Card Container
            var card = new RoundedPanel {
                Size = new Size(460, 550),
                Radius = 20,
                FillColor = ThemeManager.Paper,
                BorderColor = ThemeManager.Line,
                Padding = new Padding(36, 32, 36, 32)
            };
            Controls.Add(card);

            void CenterCard() {
                card.Left = (ClientSize.Width - card.Width) / 2;
                card.Top = (ClientSize.Height - card.Height) / 2;
            }
            Resize += (s, e) => CenterCard();
            CenterCard();

            // 1. Logo Badge
            var logoBadge = new Label {
                Text = "🍽️",
                Font = new Font("Segoe UI Emoji", 20F),
                Size = new Size(54, 54),
                Location = new Point((card.Width - 54) / 2, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            logoBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = ThemeManager.RoundedRect(new Rectangle(0, 0, 53, 53), 16))
                using (var brush = new LinearGradientBrush(logoBadge.ClientRectangle, ThemeManager.Primary, ThemeManager.Violet, 45f)) {
                    e.Graphics.FillPath(brush, path);
                }
            };
            card.Controls.Add(logoBadge);

            // 2. Title & Subtitle
            var lblTitle = new Label {
                Text = "Chào mừng trở lại",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            var lblSub = new Label {
                Text = "Đăng nhập để truy cập phần mềm quản lý nhà hàng",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblSub);

            void AlignHeaders() {
                lblTitle.Left = (card.Width - lblTitle.Width) / 2;
                lblTitle.Top = 92;
                lblSub.Left = (card.Width - lblSub.Width) / 2;
                lblSub.Top = 126;
            }
            AlignHeaders();

            // 3. Username Input
            int inputX = 36;
            int inputW = card.Width - 72;

            var lblUser = new Label {
                Text = "Tên đăng nhập",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Location = new Point(inputX, 160),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblUser);

            _username = new TextBox {
                Location = new Point(inputX, 184),
                Width = inputW,
                Font = new Font("Segoe UI", 10.5F)
            };
            card.Controls.Add(_username);

            // 4. Password Input
            var lblPass = new Label {
                Text = "Mật khẩu",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Location = new Point(inputX, 228),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblPass);

            _password = new TextBox {
                Location = new Point(inputX, 252),
                Width = inputW,
                Font = new Font("Segoe UI", 10.5F),
                UseSystemPasswordChar = true
            };
            card.Controls.Add(_password);

            // Error Label
            _error = new Label {
                Text = "",
                Location = new Point(inputX, 292),
                Size = new Size(inputW, 22),
                ForeColor = ThemeManager.Red,
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false,
                BackColor = Color.Transparent
            };
            card.Controls.Add(_error);

            // 5. Submit Button
            var btnLogin = new Button {
                Text = "Đăng nhập hệ thống",
                Location = new Point(inputX, 320),
                Size = new Size(inputW, 44),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += async (s, e) => await SubmitLogin();
            card.Controls.Add(btnLogin);
            AcceptButton = btnLogin;

            // 6. Quick Demo Account Chips (Click to autofill)
            var lblDemo = new Label {
                Text = "Chọn tài khoản thử nghiệm nhanh (Mật khẩu: 123456):",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(inputX, 384),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblDemo);

            var demoFlow = new FlowLayoutPanel {
                Location = new Point(inputX, 406),
                Size = new Size(inputW, 90),
                BackColor = Color.Transparent,
                WrapContents = true
            };

            var demoAccounts = new[] {
                ("admin", "Admin Toàn quyền"),
                ("quanly01", "Quản lý"),
                ("thungan01", "Thu ngân"),
                ("cskh01", "CSKH")
            };

            foreach (var acc in demoAccounts) {
                var chip = new Button {
                    Text = "⚡ " + acc.Item1,
                    Tag = acc.Item1,
                    Height = 28,
                    AutoSize = true,
                    BackColor = ThemeManager.Surface2,
                    ForeColor = ThemeManager.Ink2,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    Margin = new Padding(0, 0, 6, 6),
                    Cursor = Cursors.Hand
                };
                chip.FlatAppearance.BorderColor = ThemeManager.Line;
                chip.FlatAppearance.MouseOverBackColor = ThemeManager.PrimarySoft;
                chip.Click += (s, e) => {
                    _username.Text = (string)((Button)s).Tag;
                    _password.Text = "123456";
                    _error.Visible = false;
                };
                demoFlow.Controls.Add(chip);
            }
            card.Controls.Add(demoFlow);
        }

        private async System.Threading.Tasks.Task SubmitLogin() {
            string username = _username.Text.Trim();
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(_password.Text)) {
                ShowError("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return;
            }

            var result = await _authService.LoginAsync(username, _password.Text);

            if (result == null || result.UserInfo == null) {
                ShowError(_authService.LastError ?? "Tên đăng nhập hoặc mật khẩu không chính xác.");
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

            UserSession.CurrentUserId = result.UserInfo.Id;
            UserSession.CurrentUsername = result.UserInfo.Username;
            UserSession.CurrentDisplayName = result.UserInfo.HoTen;
            UserSession.CurrentRoles = roles;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowError(string message) {
            _error.Text = message;
            _error.Visible = true;
        }
    }
}

