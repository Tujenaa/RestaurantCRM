using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Services;

namespace RestaurantCRM.AdminApp.Forms.Auth
{
    public class LoginForm : Form
    {
        private readonly TextBox _username;
        private readonly TextBox _password;
        private readonly Label _error;
        private readonly AuthService _authService;

        public string Username { get; private set; }
        public string DisplayName { get; private set; }
        public List<string> Roles { get; private set; }

        public LoginForm()
        {
            _authService = new AuthService();
            Text = "RestaurantCRM  |  Đăng nhập hệ thống";
            ClientSize = new Size(900, 600);
            MinimumSize = new Size(800, 540);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            // ── Panel trái: Branding ──────────────────────────────────────
            var leftW = 360;
            var pnlLeft = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(leftW, ClientSize.Height),
                BackColor = ThemeManager.Primary
            };
            pnlLeft.Paint += (s, e) => PaintLeftBrand(e.Graphics, pnlLeft.Size);
            Controls.Add(pnlLeft);

            // ── Panel phải: Form ──────────────────────────────────────────
            var pnlRight = new Panel
            {
                Location = new Point(leftW, 0),
                Size = new Size(ClientSize.Width - leftW, ClientSize.Height),
                BackColor = Color.White
            };
            Controls.Add(pnlRight);

            int pw = pnlRight.Width;
            int formX = 52;
            int inputW = pw - formX * 2;

            // Tiêu đề form
            var lblHello = new Label
            {
                Text = "Đăng nhập",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(formX, 80),
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblHello);

            var lblSub = new Label
            {
                Text = "Nhập thông tin để truy cập hệ thống",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(formX, 118),
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblSub);

            // Divider
            var div = new Panel
            {
                Location = new Point(formX, 148),
                Size = new Size(40, 3),
                BackColor = ThemeManager.Primary
            };
            pnlRight.Controls.Add(div);

            // Username
            var lblUser = new Label
            {
                Text = "Tên đăng nhập",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Location = new Point(formX, 172),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblUser);

            _username = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5F),
                Location = new Point(formX + 36, 202),
                Width = inputW - 36 - 8
            };
            var boxUser = MakeInputBox(pnlRight, formX, 192, inputW, "👤", _username);

            // Password
            var lblPass = new Label
            {
                Text = "Mật khẩu",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Location = new Point(formX, 258),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblPass);

            _password = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10.5F),
                UseSystemPasswordChar = true,
                Location = new Point(formX + 36, 288),
                Width = inputW - 36 - 8
            };
            var boxPass = MakeInputBox(pnlRight, formX, 278, inputW, "🔒", _password);

            // Khi focus vào ô → đổi màu viền
            Action<Panel, bool> highlight = (box, active) => {
                box.Tag = active;
                box.Invalidate();
            };
            _username.Enter += (s, e) => highlight(boxUser, true);
            _username.Leave += (s, e) => highlight(boxUser, false);
            _password.Enter += (s, e) => highlight(boxPass, true);
            _password.Leave += (s, e) => highlight(boxPass, false);

            // Error
            _error = new Label
            {
                Text = "",
                Location = new Point(formX, 336),
                Size = new Size(inputW, 20),
                ForeColor = ThemeManager.Red,
                Font = new Font("Segoe UI", 8.5F),
                Visible = false,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(_error);

            // Nút đăng nhập
            var btnLogin = new Button
            {
                Text = "Đăng nhập  →",
                Location = new Point(formX, 364),
                Size = new Size(inputW, 46),
                BackColor = ThemeManager.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            // gradient hover
            btnLogin.Paint += (s, e) => {
                var b = (Button)s;
                var rc = new Rectangle(0, 0, b.Width, b.Height);
                using (var br = new LinearGradientBrush(rc, ThemeManager.Primary, ThemeManager.Violet, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(br, rc);
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, rc, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            btnLogin.Click += async (s, e) => await SubmitLogin();
            pnlRight.Controls.Add(btnLogin);
            AcceptButton = btnLogin;

            // Demo chips
            var lblDemo = new Label
            {
                Text = "Đăng nhập nhanh (mật khẩu: 123456)",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                Location = new Point(formX, 426),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlRight.Controls.Add(lblDemo);

            var demoFlow = new FlowLayoutPanel
            {
                Location = new Point(formX, 448),
                Size = new Size(inputW, 80),
                BackColor = Color.Transparent,
                WrapContents = true
            };

            var demoAccounts = new[] {
                ("admin",    "🛡 Admin"),
                ("quanly01", "👔 Quản lý"),
                ("thungan01","💰 Thu ngân"),
                ("cskh01",   "🎧 CSKH")
            };
            foreach (var acc in demoAccounts)
            {
                string user = acc.Item1, label = acc.Item2;
                var chip = new Button
                {
                    Text = label,
                    Height = 30,
                    AutoSize = true,
                    BackColor = Color.FromArgb(243, 244, 246),
                    ForeColor = ThemeManager.Ink,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5F),
                    Margin = new Padding(0, 0, 8, 8),
                    Cursor = Cursors.Hand
                };
                chip.FlatAppearance.BorderColor = ThemeManager.Line;
                chip.FlatAppearance.MouseOverBackColor = ThemeManager.PrimarySoft;
                chip.Click += (s, e) => {
                    _username.Text = user;
                    _password.Text = "123456";
                    _error.Visible = false;
                };
                demoFlow.Controls.Add(chip);
            }
            pnlRight.Controls.Add(demoFlow);
        }

        // ── Vẽ panel trái brand ───────────────────────────────────────────
        private void PaintLeftBrand(Graphics g, Size sz)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Nền gradient
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, sz.Width, sz.Height),
                ThemeManager.Primary, ThemeManager.Violet, LinearGradientMode.ForwardDiagonal))
                g.FillRectangle(br, 0, 0, sz.Width, sz.Height);

            // Vòng tròn trang trí góc trên phải
            using (var br = new SolidBrush(Color.FromArgb(30, 255, 255, 255)))
            {
                g.FillEllipse(br, sz.Width - 120, -60, 200, 200);
                g.FillEllipse(br, sz.Width - 60, 80, 130, 130);
            }
            // Vòng tròn trang trí góc dưới trái
            using (var br = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
            {
                g.FillEllipse(br, -80, sz.Height - 180, 220, 220);
                g.FillEllipse(br, 40, sz.Height - 100, 150, 150);
            }

            // Logo badge
            var badgeRect = new Rectangle((sz.Width - 64) / 2, 130, 64, 64);
            using (var path = ThemeManager.RoundedRect(badgeRect, 18))
            using (var br = new SolidBrush(Color.FromArgb(50, 255, 255, 255)))
                g.FillPath(br, path);
            using (var pen = new Pen(Color.FromArgb(90, 255, 255, 255), 1.5f))
            using (var path = ThemeManager.RoundedRect(badgeRect, 18))
                g.DrawPath(pen, path);
            using (var f = new Font(ThemeManager.EmojiFont, 22F))
                TextRenderer.DrawText(g, "🍽️", f, badgeRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // App name
            using (var fBold = new Font("Segoe UI", 20F, FontStyle.Bold))
                TextRenderer.DrawText(g, "LK CORP", fBold,
                    new Rectangle(0, 212, sz.Width, 36), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            using (var fSub = new Font("Segoe UI", 10F))
                TextRenderer.DrawText(g, "Restaurant CRM", fSub,
                    new Rectangle(0, 252, sz.Width, 24), Color.FromArgb(200, 255, 255, 255),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            // Divider
            using (var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
                g.DrawLine(pen, sz.Width / 2 - 50, 292, sz.Width / 2 + 50, 292);

            // Tagline
            using (var fTag = new Font("Segoe UI", 9F, FontStyle.Italic))
                TextRenderer.DrawText(g, "Quản lý thông minh — Phục vụ xuất sắc", fTag,
                    new Rectangle(20, 306, sz.Width - 40, 44), Color.FromArgb(180, 255, 255, 255),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);

            // Chấm trang trí nhỏ
            using (var br = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
            {
                for (int i = 0; i < 5; i++)
                    g.FillEllipse(br, 28 + i * 22, sz.Height - 60, 10, 10);
            }
        }

        // ── Tạo input box có icon + viền tuỳ chỉnh ───────────────────────
        private Panel MakeInputBox(Panel parent, int x, int y, int w, string icon, TextBox tb)
        {
            var box = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(w, 44),
                BackColor = Color.White
            };
            box.Paint += (s, e) => {
                bool active = box.Tag is bool b && b;
                var rc = new Rectangle(0, 0, box.Width - 1, box.Height - 1);
                using (var pen = new Pen(active ? ThemeManager.Primary : ThemeManager.Line, active ? 1.8f : 1f))
                    e.Graphics.DrawRectangle(pen, rc);
            };

            // Icon trái
            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 11F),
                ForeColor = ThemeManager.Muted,
                Size = new Size(34, 44),
                Location = new Point(0, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            box.Controls.Add(lblIcon);

            // Đường kẻ dọc ngăn icon và textbox
            var sep = new Panel { Location = new Point(34, 8), Size = new Size(1, 28), BackColor = ThemeManager.Line };
            box.Controls.Add(sep);

            tb.Location = new Point(44, (44 - tb.Height) / 2 + 1);
            tb.Width = w - 52;
            tb.BackColor = Color.White;
            box.Controls.Add(tb);

            parent.Controls.Add(box);
            return box;
        }

        // ── Xử lý đăng nhập ──────────────────────────────────────────────
        private async System.Threading.Tasks.Task SubmitLogin()
        {
            string username = _username.Text.Trim();
            if (String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(_password.Text))
            {
                ShowError("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");
                return;
            }

            var result = await _authService.LoginAsync(username, _password.Text);

            if (result == null || result.UserInfo == null)
            {
                ShowError(_authService.LastError ?? "Tên đăng nhập hoặc mật khẩu không chính xác.");
                return;
            }

            List<string> roles = new List<string>();
            var perms = result.Permissions ?? new List<string>();

            bool isAdmin = String.Equals(result.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                           String.Equals(result.Role, "admin", StringComparison.OrdinalIgnoreCase) ||
                           String.Equals(result.Role, "Quản lý", StringComparison.OrdinalIgnoreCase) ||
                           perms.Contains("ADMIN_ACCOUNT") || perms.Contains("ADMIN_ROLE") ||
                           perms.Contains("*") || perms.Contains("ALL");

            bool hasSale = isAdmin ||
                           String.Equals(result.Role, "sale", StringComparison.OrdinalIgnoreCase) ||
                           String.Equals(result.Role, "Bán hàng", StringComparison.OrdinalIgnoreCase) ||
                           perms.Exists(p => p.StartsWith("ORDER_") || p.StartsWith("MENU_") || p.StartsWith("PROMO_") || p.StartsWith("INVENTORY_") || p.StartsWith("IMPORT_") || p.StartsWith("INGREDIENT_") || p.StartsWith("REPORT_"));

            bool hasCrm = isAdmin ||
                          String.Equals(result.Role, "crm", StringComparison.OrdinalIgnoreCase) ||
                          String.Equals(result.Role, "CRM", StringComparison.OrdinalIgnoreCase) ||
                          String.Equals(result.Role, "Nhân viên CSKH", StringComparison.OrdinalIgnoreCase) ||
                          perms.Exists(p => p.StartsWith("CRM_"));

            if (isAdmin) roles.Add("Admin");
            if (hasSale) roles.Add("Bán hàng");
            if (hasCrm) roles.Add("CRM");

            if (roles.Count == 0)
            {
                roles.Add(!string.IsNullOrWhiteSpace(result.Role) ? result.Role : "Bán hàng");
            }

            Username = result.UserInfo.Username;
            DisplayName = result.UserInfo.HoTen;
            Roles = roles;

            UserSession.CurrentUserId = result.UserInfo.Id;
            UserSession.CurrentUsername = result.UserInfo.Username;
            UserSession.CurrentDisplayName = result.UserInfo.HoTen;
            UserSession.CurrentRoles = roles;
            UserSession.Permissions = new HashSet<string>(result.Permissions ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void ShowError(string message)
        {
            _error.Text = "⚠  " + message;
            _error.Visible = true;
        }
    }
}