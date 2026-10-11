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

        public UcHeader() {
            Height = 70;
            Dock = DockStyle.Top;
            BackColor = ThemeManager.Paper;
            Padding = new Padding(28, 12, 28, 12);

            BreadcrumbLabel = new Label {
                Text = "TRANG QUẢN TRỊ  ·  TỔNG QUAN",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = ThemeManager.Primary,
                AutoSize = true,
                Location = new Point(28, 14)
            };
            Controls.Add(BreadcrumbLabel);

            TitleLabel = new Label {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = ThemeManager.Ink,
                AutoSize = true,
                Location = new Point(27, 32)
            };
            Controls.Add(TitleLabel);

            _lblDate = new Label {
                Text = DateTime.Now.ToString("dddd, dd/MM/yyyy"),
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.Muted,
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Width - 180, 28)
            };
            Controls.Add(_lblDate);

            Resize += (s, e) => {
                _lblDate.Left = ClientSize.Width - _lblDate.Width - 28;
            };

            Paint += (s, e) => {
                using (var pen = new Pen(ThemeManager.Line, 1)) {
                    e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
                }
            };
        }

        public void SetTitle(string title, string section = "HỆ THỐNG") {
            TitleLabel.Text = title;
            BreadcrumbLabel.Text = "TRANG QUẢN TRỊ  ·  " + section.ToUpper();
        }
    }
}
