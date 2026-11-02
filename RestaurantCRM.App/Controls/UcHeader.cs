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
            Height = 60;
            Dock = DockStyle.Top;
            BackColor = ThemeManager.Bg;
            DoubleBuffered = true;

            BreadcrumbLabel = new Label
            {
                Text = "Tổng quan",
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(24, 8),
                BackColor = Color.Transparent
            };
            Controls.Add(BreadcrumbLabel);

            TitleLabel = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(23, 27),
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

            int right = Width - 24;

            // Avatar tròn
            var av = new Rectangle(right - 34, (Height - 34) / 2, 34, 34);
            using (var br = new SolidBrush(ThemeManager.AvatarColor(_userName))) g.FillEllipse(br, av);
            using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, ThemeManager.Initials(_userName), f, av, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Tên + vai trò (căn giữa theo chiều dọc, có khoảng cách thoáng giữa 2 dòng)
            using (var f1 = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var f2 = new Font("Segoe UI", 8F))
            {
                var nameSize = TextRenderer.MeasureText(g, _userName, f1, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                var roleSize = TextRenderer.MeasureText(g, _userRole, f2, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int tw = Math.Max(nameSize.Width, roleSize.Width);
                int tx = av.Left - 10 - tw;
                int totalTextH = nameSize.Height + roleSize.Height + 4;
                int startY = (Height - totalTextH) / 2;
                TextRenderer.DrawText(g, _userName, f1, new Point(tx + tw - nameSize.Width, startY), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, _userRole, f2, new Point(tx + tw - roleSize.Width, startY + nameSize.Height + 4), ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                right = tx - 14;
            }

            // Pill ngày
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            using (var fi = ThemeManager.Icon(8F))
            using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                var ts = TextRenderer.MeasureText(g, date, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                int pillH = 28;
                var pill = new Rectangle(right - ts.Width - 36, (Height - pillH) / 2, ts.Width + 36, pillH);
                using (var path = ThemeManager.RoundedRect(pill, 14))
                using (var br = new SolidBrush(Color.White))
                    g.FillPath(br, path);
                TextRenderer.DrawText(g, ThemeManager.Glyph.Calendar, fi, new Rectangle(pill.X + 8, pill.Y, 14, pill.Height),
                    ThemeManager.Primary, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                TextRenderer.DrawText(g, date, f, new Rectangle(pill.X + 26, pill.Y, ts.Width + 4, pill.Height),
                    ThemeManager.Primary, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }

        public void SetUserInfo(string displayName, string role)
        {
            _userName = displayName ?? "";
            _userRole = role == null ? "" : "Phân hệ " + ThemeManager.DisplayWorkspaceName(role);
            Invalidate();
        }

        public void SetTitle(string title, string section = "Hệ thống")
        {
            TitleLabel.Text = title;
            BreadcrumbLabel.Text = "Phân hệ " + ThemeManager.DisplayWorkspaceName(section) + "  /  " + title;
            Invalidate();
        }
    }
}
