using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls {
    /// <summary>Thẻ thống kê: tiêu đề, giá trị lớn, mô tả và biểu tượng tròn màu nhấn.</summary>
    public class UcThongKe : RoundedPanel {
        private readonly string _icon;
        private readonly Color _accent;
        private readonly Label _lblValue;
        private readonly Label _lblSub;
        private readonly Label _lblTitle;

        public UcThongKe(string label, string value, string sub) : this(label, value, sub, "📊", ThemeManager.Primary) { }

        public UcThongKe(string label, string value, string sub, string icon, Color accent) {
            _icon = icon; _accent = accent;
            Size = new Size(240, 126);
            Margin = new Padding(0, 0, 18, 0);
            Radius = 16;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(180, ThemeManager.Line);

            _lblTitle = new Label {
                Text = label,
                ForeColor = ThemeManager.Muted,
                Location = new Point(18, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F),
                BackColor = Color.Transparent
            };
            Controls.Add(_lblTitle);

            _lblValue = new Label {
                Text = value,
                ForeColor = ThemeManager.Ink,
                Location = new Point(15, 36),
                AutoSize = true,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            Controls.Add(_lblValue);

            _lblSub = new Label {
                Text = "▲ " + sub,
                ForeColor = accent,
                Location = new Point(18, 92),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            Controls.Add(_lblSub);
        }

        public void SetValue(string val, string sub = null) {
            if (_lblValue != null) _lblValue.Text = val;
            if (sub != null && _lblSub != null) _lblSub.Text = "▲ " + sub;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Icon badge (circle gradient)
            var badge = new Rectangle(Width - 68, 14, 50, 50);
            using (var path = ThemeManager.RoundedRect(badge, 14))
            using (var brush = new LinearGradientBrush(badge, ThemeManager.SoftFor(_accent), Color.FromArgb(220, ThemeManager.SoftFor(_accent)), 135f))
                g.FillPath(brush, path);

            using (var pen = new Pen(Color.FromArgb(60, _accent), 1.2f))
            using (var path = ThemeManager.RoundedRect(badge, 14))
                g.DrawPath(pen, path);

            using (var f = new Font(ThemeManager.EmojiFont, 16F))
                TextRenderer.DrawText(g, _icon, f, badge, _accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // Bottom accent bar
            using (var brush = new LinearGradientBrush(
                new Rectangle(0, Height - 4, Width, 4),
                _accent, ThemeManager.Lighten(_accent, 0.4f), LinearGradientMode.Horizontal))
            using (var path = ThemeManager.RoundedRect(new Rectangle(16, Height - 5, Width - 32, 4), 2))
                g.FillPath(brush, path);
        }
    }
}
