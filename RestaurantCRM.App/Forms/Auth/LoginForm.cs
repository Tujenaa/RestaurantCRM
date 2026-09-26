using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Forms.Dashboard;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.Auth {
    public partial class LoginForm : Form {
        private readonly Panel _launcher;
        private readonly Panel _loginPanel;
        private readonly TextBox _username;
        private readonly TextBox _password;
        private readonly Label _loginTitle;
        private readonly Label _error;
        private string _selectedRole = "Admin";

        public LoginForm() {
            Text = "RestaurantCRM | Đăng nhập"; Size = new Size(1100, 720); MinimumSize = new Size(920, 620);
            StartPosition = FormStartPosition.CenterScreen; BackColor = ThemeManager.Bg; Font = new Font("Segoe UI", 9.5F);
            _launcher = new Panel { Dock = DockStyle.Fill, Padding = new Padding(34), BackColor = ThemeManager.Bg }; Controls.Add(_launcher);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = ThemeManager.Bg };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            _launcher.Controls.Add(layout);
            var hero = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(23, 61, 114), Padding = new Padding(28), Margin = new Padding(0) };
            hero.Controls.Add(new Label { Text = "HỆ THỐNG QUẢN LÝ CHUỖI NHÀ HÀNG", ForeColor = Color.White, Font = new Font("Segoe UI", 23F, FontStyle.Bold), AutoSize = true, Location = new Point(26, 25) });
            hero.Controls.Add(new Label { Text = "Chọn phân hệ để đăng nhập và làm việc.", ForeColor = Color.FromArgb(220, 233, 255), Font = new Font("Segoe UI", 11F), AutoSize = true, Location = new Point(30, 76) });
            layout.Controls.Add(hero, 0, 0);
            var cards = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(0, 22, 0, 20), BackColor = ThemeManager.Bg, Margin = new Padding(0) };
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            layout.Controls.Add(cards, 0, 1);
            AddCard(cards, 0, "◆", "Admin — Quản trị hệ thống", "Quản lý tài khoản, vai trò, món ăn, nhà cung cấp, kho và khuyến mãi.", "Đăng nhập Admin →", ThemeManager.Blue, "Admin");
            AddCard(cards, 1, "▣", "Bán hàng — Quản lý bán hàng", "Tạo và xử lý đơn hàng, tra cứu thực đơn và theo dõi báo cáo doanh thu.", "Đăng nhập Bán hàng →", Color.FromArgb(14, 124, 134), "Bán hàng");
            AddCard(cards, 2, "●", "CRM — Quan hệ khách hàng", "Quản lý hồ sơ, đánh giá, phản hồi, khảo sát và phân tích khách hàng.", "Đăng nhập CRM →", ThemeManager.Purple, "CRM");
            var footer = new Label { Text = "Giao diện demo. Đăng nhập hiện kiểm tra thông tin nhập và chưa xác thực với API.", Dock = DockStyle.Bottom, Height = 24, ForeColor = ThemeManager.Muted, TextAlign = ContentAlignment.MiddleLeft };
            layout.Controls.Add(footer, 0, 2);

            _loginPanel = new Panel { Visible = false, Size = new Size(430, 440), BackColor = ThemeManager.Paper, Padding = new Padding(32) }; Controls.Add(_loginPanel);
            _loginTitle = new Label { Text = "Đăng nhập", Font = new Font("Segoe UI", 19F, FontStyle.Bold), ForeColor = ThemeManager.Ink, AutoSize = true, Location = new Point(32, 30) }; _loginPanel.Controls.Add(_loginTitle);
            _loginPanel.Controls.Add(new Label { Text = "Tên đăng nhập", Location = new Point(34, 108), AutoSize = true, ForeColor = ThemeManager.Muted });
            _username = new TextBox { Location = new Point(34, 132), Width = 356, Height = 32, Font = Font }; _loginPanel.Controls.Add(_username);
            _loginPanel.Controls.Add(new Label { Text = "Mật khẩu", Location = new Point(34, 186), AutoSize = true, ForeColor = ThemeManager.Muted });
            _password = new TextBox { Location = new Point(34, 210), Width = 356, Height = 32, Font = Font, UseSystemPasswordChar = true }; _loginPanel.Controls.Add(_password);
            _error = new Label { Text = "Vui lòng nhập tên đăng nhập và mật khẩu.", ForeColor = ThemeManager.Red, Location = new Point(34, 256), AutoSize = true, Visible = false }; _loginPanel.Controls.Add(_error);
            var login = new Button { Text = "Đăng nhập", Location = new Point(34, 292), Size = new Size(356, 42), BackColor = ThemeManager.Blue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            login.FlatAppearance.BorderSize = 0; login.Click += (s, e) => SubmitLogin(); _loginPanel.Controls.Add(login);
            var back = new Button { Text = "← Quay lại chọn phân hệ", Location = new Point(34, 346), Size = new Size(356, 38), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = ThemeManager.Ink };
            back.Click += (s, e) => ShowLauncher(); _loginPanel.Controls.Add(back);
            AcceptButton = login;
        }

        private void AddCard(TableLayoutPanel host, int column, string glyph, string title, string description, string buttonText, Color accent, string role) {
            var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 14, 0), BackColor = ThemeManager.Paper, Padding = new Padding(20) };
            card.Paint += (s, e) => { using (var pen = new Pen(ThemeManager.Line)) e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1); };
            card.Controls.Add(new Label { Text = glyph, Font = new Font("Segoe UI", 28F, FontStyle.Bold), ForeColor = accent, AutoSize = true, Location = new Point(20, 22) });
            card.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = ThemeManager.Ink, Location = new Point(20, 82), Size = new Size(275, 58) });
            card.Controls.Add(new Label { Text = description, Font = new Font("Segoe UI", 9F), ForeColor = ThemeManager.Muted, Location = new Point(22, 150), Size = new Size(270, 88) });
            var button = new Button { Text = buttonText, BackColor = accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Size = new Size(270, 42), Location = new Point(20, 262), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderSize = 0; button.Click += (s, e) => ShowLogin(role, accent); card.Controls.Add(button); host.Controls.Add(card, column, 0);
        }

        private void ShowLogin(string role, Color accent) {
            _selectedRole = role; _loginTitle.Text = "Đăng nhập " + role; _error.Visible = false; _username.Clear(); _password.Clear();
            foreach (Control c in _loginPanel.Controls) if (c is Button && c.Text == "Đăng nhập") c.BackColor = accent;
            _launcher.Visible = false; _loginPanel.Visible = true; _loginPanel.Left = (ClientSize.Width - _loginPanel.Width) / 2; _loginPanel.Top = (ClientSize.Height - _loginPanel.Height) / 2; _loginPanel.BringToFront(); _username.Focus();
        }

        private void ShowLauncher() { _loginPanel.Visible = false; _launcher.Visible = true; _launcher.BringToFront(); }

        private void SubmitLogin() {
            if (String.IsNullOrWhiteSpace(_username.Text) || String.IsNullOrWhiteSpace(_password.Text)) { _error.Visible = true; return; }
            Hide();
            using (var workspace = new DashboardForm(_selectedRole)) workspace.ShowDialog(this);
            if (!IsDisposed) { Show(); ShowLauncher(); }
        }
    }
}
