using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Thẻ thống kê tối giản: số lớn, nhãn nhỏ viết hoa, dòng chỉ báo có vòng tròn mũi tên.
    /// Tự vẽ toàn bộ để chữ sắc nét và không bị nhấp nháy.
    /// </summary>
    public class UcThongKe : RoundedPanel
    {
        private readonly string _icon;
        private readonly Color _accent;
        private readonly string _label;
        private string _value;
        private string _sub;

        public UcThongKe(string label, string value, string sub) : this(label, value, sub, "📊", ThemeManager.Primary) { }

        public UcThongKe(string label, string value, string sub, string icon, Color accent)
        {
            _icon = icon; _accent = accent; _label = label; _value = value; _sub = sub;
            Size = new Size(220, 128);
            Margin = new Padding(0);
            Radius = 14;
            Shadow = 8;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(120, ThemeManager.Line);
        }

        public void SetValue(string val, string sub = null)
        {
            _value = val;
            if (sub != null) _sub = sub;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var card = CardRect;
            int x = card.X + 20;

            // Giá trị lớn
            using (var f = new Font("Segoe UI", 19F, FontStyle.Bold))
                TextRenderer.DrawText(g, _value ?? "", f, new Rectangle(x, card.Y + 16, card.Width - 76, 38),
                    ThemeManager.Ink, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Nhãn nhỏ viết hoa
            using (var f = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, (_label ?? "").ToUpper(), f, new Rectangle(x, card.Y + 56, card.Width - 40, 16),
                    ThemeManager.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Icon nhỏ góc phải trong ô vuông màu nhạt
            var badge = new Rectangle(card.Right - 52, card.Y + 16, 36, 36);
            using (var path = ThemeManager.RoundedRect(badge, 10))
            using (var br = new SolidBrush(ThemeManager.SoftFor(_accent)))
                g.FillPath(br, path);
            using (var f = new Font(ThemeManager.EmojiFont, 12F))
                TextRenderer.DrawText(g, _icon, f, badge, _accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Dòng chỉ báo: vòng tròn mũi tên + mô tả
            int iy = card.Bottom - 34;
            var circle = new Rectangle(x, iy, 20, 20);
            using (var pen = new Pen(_accent, 1.4f)) g.DrawEllipse(pen, circle);
            using (var f = ThemeManager.Icon(7F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.Up, f, circle, _accent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, _sub ?? "", f, new Rectangle(x + 28, iy, card.Width - 56, 20),
                    _accent, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}