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

        public UcThongKe(string label, string value, string sub) : this(label, value, sub, "📊", ThemeManager.Primary) { }

        public UcThongKe(string label, string value, string sub, string icon, Color accent) {
            _icon = icon; _accent = accent;
            Size = new Size(236, 122);
            Margin = new Padding(0, 0, 18, 18);
            Radius = 16;
            FillColor = ThemeManager.Paper;
            BorderColor = ThemeManager.Line;

            Controls.Add(new Label { Text = label, ForeColor = ThemeManager.Muted, Location = new Point(18, 18), AutoSize = true, Font = ThemeManager.Body(9F), BackColor = Color.Transparent });
            
            _lblValue = new Label { Text = value, ForeColor = ThemeManager.Ink, Location = new Point(15, 40), AutoSize = true, Font = ThemeManager.Body(20F, FontStyle.Bold), BackColor = Color.Transparent };
            Controls.Add(_lblValue);

            _lblSub = new Label { Text = "● " + sub, ForeColor = accent, Location = new Point(18, 88), AutoSize = true, Font = ThemeManager.Body(8.5F), BackColor = Color.Transparent };
            Controls.Add(_lblSub);
        }

        public void SetValue(string val, string sub = null) {
            if (_lblValue != null) _lblValue.Text = val;
            if (sub != null && _lblSub != null) _lblSub.Text = "● " + sub;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var badge = new Rectangle(Width - 62, 16, 44, 44);
            using (var path = ThemeManager.RoundedRect(badge, 12))
            using (var brush = new SolidBrush(ThemeManager.SoftFor(_accent)))
                g.FillPath(brush, path);
            using (var f = new Font(ThemeManager.EmojiFont, 15F))
                TextRenderer.DrawText(g, _icon, f, badge, _accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
