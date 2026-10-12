using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcHeader : UserControl {
        public Label TitleLabel { get; set; }
        public Label BreadcrumbLabel { get; set; }
        private Label _lblDate;
        private Color _accentColor = ThemeManager.Primary;

        public UcHeader() {
            Height = 70;
            Dock = DockStyle.Top;
            BackColor = ThemeManager.Paper;
            DoubleBuffered = true;

            BreadcrumbLabel = new Label {
                Text = "TRANG QUẢN TRỊ  ·  TỔNG QUAN",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                AutoSize = true,
                Location = new Point(28, 12)
            };
            Controls.Add(BreadcrumbLabel);

            TitleLabel = new Label {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(26, 28)
            };
            Controls.Add(TitleLabel);

            _lblDate = new Label {
                Text = "📅  " + DateTime.Now.ToString("dddd, dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Width - 220, 28)
            };
            Controls.Add(_lblDate);

            Resize += (s, e) => {
                _lblDate.Left = ClientSize.Width - _lblDate.Width - 28;
            };

            Paint += (s, e) => {
                var g = e.Graphics;
                // Bottom border line
                using (var pen = new Pen(ThemeManager.Line, 1))
                    g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

                // Left accent bar
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 4, Height),
                    _accentColor,
                    ThemeManager.Violet,
                    LinearGradientMode.Vertical))
                    g.FillRectangle(brush, 0, 0, 4, Height);
            };
        }

        public void SetTitle(string title, string section = "HỆ THỐNG") {
            TitleLabel.Text = title;
            BreadcrumbLabel.Text = "TRANG QUẢN TRỊ  ·  " + section.ToUpper();
            _accentColor = ThemeManager.Primary;
            if (section == "CRM") _accentColor = ThemeManager.Violet;
            else if (section == "BÁN HÀNG") _accentColor = ThemeManager.Teal;
            Invalidate();
        }
    }
}
