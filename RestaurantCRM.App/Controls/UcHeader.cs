using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Header tối giản nằm trên nền xám của vùng nội dung:
    /// breadcrumb nhỏ + tiêu đề trang bên trái, ngày và avatar người dùng bên phải.
    /// </summary>
    public partial class UcHeader : UserControl
    {
        public Label TitleLabel { get; set; }
        public Label BreadcrumbLabel { get; set; }
        private string _userName = "";
        private string _userRole = "";

        public UcHeader()
        {
            Height = 76;
            Dock = DockStyle.Top;
            BackColor = ThemeManager.Bg;
            DoubleBuffered = true;

            BreadcrumbLabel = new Label
            {
                Text = "Tổng quan",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(30, 16),
                BackColor = Color.Transparent
            };
            Controls.Add(BreadcrumbLabel);

            TitleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(27, 34),
                BackColor = Color.Transparent
            };
            Controls.Add(TitleLabel);

            Resize += (s, e) => Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int right = Width - 30;

            // Avatar tròn
            var av = new Rectangle(right - 36, 22, 36, 36);
            using (var br = new SolidBrush(ThemeManager.AvatarColor(_userName))) g.FillEllipse(br, av);
            using (var f = new Font("Segoe UI", 9F, FontStyle.Bold))
                TextRenderer.DrawText(g, ThemeManager.Initials(_userName), f, av, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Tên + vai trò
            using (var f1 = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var f2 = new Font("Segoe UI", 8F))
            {
                var nameSize = TextRenderer.MeasureText(g, _userName, f1, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                var roleSize = TextRenderer.MeasureText(g, _userRole, f2, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int tw = Math.Max(nameSize.Width, roleSize.Width);
                int tx = av.Left - 12 - tw;
                TextRenderer.DrawText(g, _userName, f1, new Point(tx + tw - nameSize.Width, 24), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, _userRole, f2, new Point(tx + tw - roleSize.Width, 42), ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                right = tx - 18;
            }

            // Pill ngày
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            using (var fi = ThemeManager.Icon(8.5F))
            using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                var ts = TextRenderer.MeasureText(g, date, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                var pill = new Rectangle(right - ts.Width - 44, 24, ts.Width + 44, 32);
                using (var path = ThemeManager.RoundedRect(pill, 16))
                using (var br = new SolidBrush(Color.White))
                    g.FillPath(br, path);
                TextRenderer.DrawText(g, ThemeManager.Glyph.Calendar, fi, new Rectangle(pill.X + 12, pill.Y, 16, pill.Height),
                    ThemeManager.Primary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, date, f, new Rectangle(pill.X + 32, pill.Y, ts.Width + 4, pill.Height),
                    ThemeManager.Primary, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        public void SetUserInfo(string displayName, string role)
        {
            _userName = displayName ?? "";
            _userRole = role == null ? "" : "Phân hệ " + role;
            Invalidate();
        }

        public void SetTitle(string title, string section = "Hệ thống")
        {
            TitleLabel.Text = title;
            BreadcrumbLabel.Text = "Phân hệ " + section + "  /  " + title;
            Invalidate();
        }
    }
}