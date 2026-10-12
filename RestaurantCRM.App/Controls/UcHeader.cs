using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    public partial class UcHeader : UserControl {
        public Label TitleLabel { get; set; }
        public Label BreadcrumbLabel { get; set; }
        private Label _lblUserInfo;
        private Label _lblDate;
        private Color _accentColor = ThemeManager.Primary;

        public UcHeader() {
            Height = 68;
            Dock = DockStyle.Top;
            BackColor = ThemeManager.Paper;
            DoubleBuffered = true;

            BreadcrumbLabel = new Label {
                Text = "TRANG QUẢN TRỊ  ·  TỔNG QUAN",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                AutoSize = true,
                Location = new Point(24, 12)
            };
            Controls.Add(BreadcrumbLabel);

            TitleLabel = new Label {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(22, 28)
            };
            Controls.Add(TitleLabel);

            // Thông tin tài khoản đăng nhập (ở góc phải phía trên)
            _lblUserInfo = new Label {
                Text = "👤 Đang tải...",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(Width - 220, 14)
            };
            Controls.Add(_lblUserInfo);

            // Ngày giờ hiện tại (ở góc phải phía dưới, không bao giờ bị đè chữ)
            _lblDate = new Label {
                Text = "📅  " + DateTime.Now.ToString("dddd, dd/MM/yyyy"),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Location = new Point(Width - 220, 36)
            };
            Controls.Add(_lblDate);

            Resize += (s, e) => RepositionRightLabels();

            Paint += (s, e) => {
                var g = e.Graphics;
                // Đường viền đáy thanh mảnh
                using (var pen = new Pen(ThemeManager.Line, 1))
                    g.DrawLine(pen, 0, Height - 1, Width, Height - 1);

                // Dải màu nhấn tinh tế bên trái
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, 4, Height),
                    _accentColor,
                    ThemeManager.Violet,
                    LinearGradientMode.Vertical))
                    g.FillRectangle(brush, 0, 0, 4, Height);
            };
        }

        private void RepositionRightLabels() {
            if (_lblUserInfo != null)
                _lblUserInfo.Left = ClientSize.Width - _lblUserInfo.Width - 24;
            if (_lblDate != null)
                _lblDate.Left = ClientSize.Width - _lblDate.Width - 24;
        }

        public void SetUserInfo(string displayName, string role) {
            _lblUserInfo.Text = "👤 " + displayName + "  (" + role + ")";
            RepositionRightLabels();
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
