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
    /// <summary>
    /// Màn đăng nhập cùng phong cách với ứng dụng: nền xám nhạt, bên trái giới thiệu + hình minh họa
    /// (thẻ số liệu, biểu đồ), bên phải thẻ đăng nhập trắng bo góc.
    /// </summary>
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
            ClientSize = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ThemeManager.Bg;
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;

            // ── Bên trái: giới thiệu + minh họa (tự vẽ) ─────────────────
            var left = new Panel { Location = new Point(0, 0), Size = new Size(500, ClientSize.Height), BackColor = ThemeManager.Bg };
            left.Paint += (s, e) => PaintShowcase(e.Graphics, left.Size);
            SkinUtil.EnableDoubleBuffer(left);
            Controls.Add(left);

            // ── Bên phải: thẻ đăng nhập ────────────────────────────────
            var card = PageKit.NewCard();
            card.Size = new Size(420, 520);
            card.Location = new Point(500, (ClientSize.Height - card.Height) / 2 + 4);
            Controls.Add(card);

            int x = 8 + 36, w = card.Width - 2 * x;

            var lblTitle = new Label
            {
                Text = "Đăng nhập",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(x - 3, 40),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Chào mừng trở lại! Nhập thông tin để tiếp tục.",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(x, 82),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblSub);

            card.Controls.Add(MakeCaption("Tên đăng nhập", x, 124));
            _username = new TextBox();
            card.Controls.Add(new LoginField(_username, ThemeManager.Glyph.Contact) { Location = new Point(x, 146), Size = new Size(w, 44) });

            card.Controls.Add(MakeCaption("Mật khẩu", x, 204));
            _password = new TextBox { UseSystemPasswordChar = true };
            card.Controls.Add(new LoginField(_password, "\uE72E") { Location = new Point(x, 226), Size = new Size(w, 44) });

            _error = new Label
            {
                Text = "",
                Location = new Point(x, 278),
                Size = new Size(w, 36),
                ForeColor = ThemeManager.Red,
                Font = new Font("Segoe UI", 8.5F),
                Visible = false,
                BackColor = Color.Transparent
            };
            card.Controls.Add(_error);

            var btnLogin = new Button
            {
                Text = "Đăng nhập",
                Location = new Point(x, 318),
                Size = new Size(w, 46),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
            };
            ButtonSkin.SetKind(btnLogin, ButtonSkin.Kind.Primary);
            btnLogin.Click += async (s, e) => await SubmitLogin();
            card.Controls.Add(btnLogin);
            AcceptButton = btnLogin;

            // Đăng nhập nhanh (tài khoản mẫu trong SampleData.sql, mật khẩu 123456)
            var lblDemo = new Label
            {
                Text = "ĐĂNG NHẬP NHANH  ·  MẬT KHẨU 123456",
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                ForeColor = ThemeManager.Muted,
                Location = new Point(x, 388),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblDemo);

            var demoAccounts = new[] {
                ("admin",     "Admin"),
                ("quanly01",  "Quản lý"),
                ("thungan01", "Thu ngân"),
                ("thungan02", "CSKH")
            };
            int cx = x;
            foreach (var acc in demoAccounts)
            {
                string user = acc.Item1;
                var chip = new Button { Text = acc.Item2, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold) };
                ButtonSkin.SetKind(chip, ButtonSkin.Kind.Soft);
                int cw = TextRenderer.MeasureText(acc.Item2, chip.Font).Width + 30;
                chip.SetBounds(cx, 412, cw, 32);
                cx += cw + 8;
                chip.Click += (s, e) =>
                {
                    _username.Text = user;
                    _password.Text = "123456";
                    _error.Visible = false;
                    _password.Focus();
                };
                card.Controls.Add(chip);
            }

            var lblFoot = new Label
            {
                Text = "© " + DateTime.Now.Year + " LK Corp · Restaurant CRM",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(x, 470),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblFoot);

            Shown += (s, e) => _username.Focus();
        }

        private static Label MakeCaption(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink2,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        /// <summary>Ô nhập bo góc có icon bên trái, viền xanh khi đang nhập.</summary>
        private class LoginField : Control
        {
            private readonly TextBox _tb;
            private readonly string _glyph;
            private bool _focus;
            public LoginField(TextBox tb, string glyph)
            {
                _tb = tb; _glyph = glyph;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                tb.BorderStyle = BorderStyle.None;
                tb.BackColor = ThemeManager.Surface2;
                tb.ForeColor = ThemeManager.Ink;
                tb.Font = new Font("Segoe UI", 10.5F);
                tb.Tag = UiStyler.NoStyleTag;
                Controls.Add(tb);
                tb.Enter += (s, e) => { _focus = true; Invalidate(); };
                tb.Leave += (s, e) => { _focus = false; Invalidate(); };
                Cursor = Cursors.IBeam;
                Click += (s, e) => tb.Focus();
            }
            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                _tb.SetBounds(46, (Height - _tb.PreferredHeight) / 2 + 1, Width - 46 - 14, _tb.PreferredHeight);
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
                using (var path = SkinUtil.Round(r, 10))
                {
                    using (var br = new SolidBrush(ThemeManager.Surface2)) g.FillPath(br, path);
                    using (var pen = new Pen(_focus ? ThemeManager.Primary : ThemeManager.Line, _focus ? 1.6f : 1f)) g.DrawPath(pen, path);
                }
                using (var f = ThemeManager.Icon(11F))
                    TextRenderer.DrawText(g, _glyph, f, new Rectangle(14, 0, 22, Height), _focus ? ThemeManager.Primary : ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        // ── Vẽ phần giới thiệu bên trái ─────────────────────────────────
        private void PaintShowcase(Graphics g, Size sz)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            int x = 56;

            // Logo
            var badge = new Rectangle(x, 44, 30, 30);
            using (var path = ThemeManager.RoundedRect(badge, 8))
            using (var br = new SolidBrush(ThemeManager.Primary)) g.FillPath(br, path);
            using (var f = ThemeManager.Icon(11F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.Home, f, badge, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            using (var f1 = new Font("Segoe UI", 14F, FontStyle.Bold))
            using (var f2 = new Font("Segoe UI", 14F))
            {
                TextRenderer.DrawText(g, "LK", f1, new Point(x + 40, 46), ThemeManager.Ink, TextFormatFlags.NoPadding);
                int lw = TextRenderer.MeasureText(g, "LK", f1, Size.Empty, TextFormatFlags.NoPadding).Width;
                TextRenderer.DrawText(g, "restaurant", f2, new Point(x + 42 + lw, 46), ThemeManager.Primary, TextFormatFlags.NoPadding);
            }

            // Khẩu hiệu
            using (var f = new Font("Segoe UI", 22F, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, "Quản lý nhà hàng", f, new Point(x - 2, 112), ThemeManager.Ink, TextFormatFlags.NoPadding);
                TextRenderer.DrawText(g, "gọn gàng mỗi ngày", f, new Point(x - 2, 152), ThemeManager.Primary, TextFormatFlags.NoPadding);
            }
            using (var f = new Font("Segoe UI", 9.5F))
                TextRenderer.DrawText(g, "Đơn hàng, kho, khuyến mãi và chăm sóc khách hàng\ntrên cùng một nền tảng.", f,
                    new Rectangle(x, 202, sz.Width - x - 30, 44), ThemeManager.Muted, TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            // Thẻ biểu đồ minh họa
            var chart = new Rectangle(x, 276, 360, 200);
            ThemeManager.DrawSoftShadow(g, chart, 14, 8);
            using (var path = ThemeManager.RoundedRect(chart, 14))
            using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
            using (var f = new Font("Segoe UI", 9.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, "Doanh thu tuần này", f, new Point(chart.X + 20, chart.Y + 18), ThemeManager.Ink, TextFormatFlags.NoPadding);
            using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
                TextRenderer.DrawText(g, "+12% SO VỚI TUẦN TRƯỚC", f, new Point(chart.X + 20, chart.Y + 40), ThemeManager.Green, TextFormatFlags.NoPadding);
            float[] v = { 0.35f, 0.5f, 0.42f, 0.7f, 0.55f, 0.82f, 0.74f };
            var plot = new RectangleF(chart.X + 22, chart.Y + 70, chart.Width - 44, chart.Height - 96);
            var pts = new PointF[v.Length];
            for (int i = 0; i < v.Length; i++) pts[i] = new PointF(plot.X + plot.Width * i / (v.Length - 1), plot.Bottom - plot.Height * v[i]);
            using (var pen = new Pen(Color.FromArgb(240, 242, 247)))
                for (int i = 0; i <= 3; i++) { float y = plot.Bottom - plot.Height * i / 3; g.DrawLine(pen, plot.X, y, plot.Right, y); }
            using (var area = new GraphicsPath())
            {
                area.AddCurve(pts, 0.45f);
                area.AddLine(pts[pts.Length - 1].X, pts[pts.Length - 1].Y, pts[pts.Length - 1].X, plot.Bottom);
                area.AddLine(pts[pts.Length - 1].X, plot.Bottom, pts[0].X, plot.Bottom);
                area.CloseFigure();
                using (var br = new LinearGradientBrush(new RectangleF(plot.X, plot.Y - 1, plot.Width, plot.Height + 2),
                    Color.FromArgb(50, ThemeManager.Primary), Color.FromArgb(0, ThemeManager.Primary), LinearGradientMode.Vertical))
                    g.FillPath(br, area);
            }
            using (var pen = new Pen(ThemeManager.Primary, 2.4f) { LineJoin = LineJoin.Round })
                g.DrawCurve(pen, pts, 0.45f);
            var dot = pts[5];
            using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, dot.X - 6, dot.Y - 6, 12, 12);
            using (var pen = new Pen(ThemeManager.Primary, 3f)) g.DrawEllipse(pen, dot.X - 5, dot.Y - 5, 10, 10);

            // Thẻ số liệu nhỏ nổi bên cạnh
            var stat = new Rectangle(x + 250, 236, 170, 76);
            ThemeManager.DrawSoftShadow(g, stat, 12, 8);
            using (var path = ThemeManager.RoundedRect(stat, 12))
            using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
            using (var f = new Font("Segoe UI", 16F, FontStyle.Bold))
                TextRenderer.DrawText(g, "128", f, new Point(stat.X + 16, stat.Y + 12), ThemeManager.Ink, TextFormatFlags.NoPadding);
            using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
                TextRenderer.DrawText(g, "KHÁCH HÀNG THÂN THIẾT", f, new Point(stat.X + 16, stat.Y + 46), ThemeManager.Muted, TextFormatFlags.NoPadding);
            var circle = new Rectangle(stat.Right - 40, stat.Y + 14, 24, 24);
            using (var pen = new Pen(ThemeManager.Green, 1.4f)) g.DrawEllipse(pen, circle);
            using (var f = ThemeManager.Icon(8F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.Up, f, circle, ThemeManager.Green,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // Pill trạng thái minh họa
            var pill = new Rectangle(x + 20, 500, 118, 28);
            using (var path = ThemeManager.RoundedRect(pill, 8))
            using (var br = new SolidBrush(ThemeManager.GreenSoft)) g.FillPath(br, path);
            using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
                TextRenderer.DrawText(g, "● Hệ thống sẵn sàng", f, pill, ThemeManager.Green, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

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
                           String.Equals(result.Role, "Hệ thống", StringComparison.OrdinalIgnoreCase) ||
                           perms.Contains("ADMIN_ACCOUNT") || perms.Contains("ADMIN_ROLE") ||
                           perms.Contains("*") || perms.Contains("ALL");

            bool hasSale = isAdmin ||
                           String.Equals(result.Role, "sale", StringComparison.OrdinalIgnoreCase) ||
                           String.Equals(result.Role, "Bán hàng", StringComparison.OrdinalIgnoreCase) ||
                           String.Equals(result.Role, "Vận Hành", StringComparison.OrdinalIgnoreCase) ||
                           String.Equals(result.Role, "Vận hành", StringComparison.OrdinalIgnoreCase) ||
                           perms.Exists(p => p.StartsWith("ORDER_") || p.StartsWith("MENU_") || p.StartsWith("PROMO_") || p.StartsWith("INVENTORY_") || p.StartsWith("IMPORT_") || p.StartsWith("INGREDIENT_") || p.StartsWith("REPORT_"));

            bool hasCrm = isAdmin ||
                          String.Equals(result.Role, "crm", StringComparison.OrdinalIgnoreCase) ||
                          String.Equals(result.Role, "CRM", StringComparison.OrdinalIgnoreCase) ||
                          String.Equals(result.Role, "Khách hàng", StringComparison.OrdinalIgnoreCase) ||
                          String.Equals(result.Role, "khách hàng", StringComparison.OrdinalIgnoreCase) ||
                          String.Equals(result.Role, "Nhân viên CSKH", StringComparison.OrdinalIgnoreCase) ||
                          perms.Exists(p => p.StartsWith("CRM_"));

            if (isAdmin) roles.Add("Hệ thống");
            if (hasSale) roles.Add("Vận Hành");
            if (hasCrm) roles.Add("Khách hàng");

            if (roles.Count == 0)
            {
                roles.Add(!string.IsNullOrWhiteSpace(result.Role) ? ThemeManager.DisplayWorkspaceName(result.Role) : "Vận Hành");
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