using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    // ───────────────────────────────────────────────────────────────────────
    //  THẺ KPI GỌN (lấy dữ liệu từ một Label nguồn: "Tiêu đề: giá trị")
    // ───────────────────────────────────────────────────────────────────────
    public class KpiTile : RoundedPanel
    {
        private string _caption = "";
        private string _value = "";
        private Color _accent;
        private readonly string _fixedCaption;
        private bool _accentAuto;

        public KpiTile(Label source, string fixedCaption = null, Color? accent = null)
        {
            _fixedCaption = fixedCaption;
            Radius = 14; Shadow = 8;
            FillColor = Color.White;
            BorderColor = Color.FromArgb(120, ThemeManager.Line);
            Margin = new Padding(0);
            DoubleBuffered = true;
            _accent = accent ?? Color.Empty;
            if (source != null)
            {
                source.Visible = false;
                if (source.Parent != null) source.Parent.Controls.Remove(source);
                Controls.Add(source);
                source.TextChanged += (s, e) => Parse(source.Text);
                Parse(source.Text);
            }
        }

        public void Parse(string text)
        {
            string t = SkinUtil.CleanText(text ?? "");
            if (_fixedCaption != null) { _caption = _fixedCaption; _value = t; }
            else
            {
                int i = t.LastIndexOf(':');
                if (i > 0) { _caption = t.Substring(0, i).Trim(); _value = t.Substring(i + 1).Trim(); }
                else { _caption = ""; _value = t; }
            }
            if (_accent == Color.Empty || _accentAuto) { _accent = AutoAccent(_caption); _accentAuto = true; }
            Invalidate();
        }

        private static Color AutoAccent(string caption)
        {
            string c = (caption ?? "").ToLowerInvariant();
            if (c.Contains("hủy") || c.Contains("hết") || c.Contains("ngừng")) return ThemeManager.Red;
            if (c.Contains("chờ") || c.Contains("sắp")) return ThemeManager.Amber;
            if (c.Contains("hoàn thành") || c.Contains("đủ") || c.Contains("đang kinh doanh") || c.Contains("đang nhập") || c.Contains("đang áp dụng")) return ThemeManager.Green;
            if (c.Contains("giao") || c.Contains("đang")) return ThemeManager.Sky;
            return ThemeManager.Primary;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = CardRect;
            if (r.Width <= 10 || r.Height <= 10) return;

            // Tính toán kích thước & căn giữa nội dung theo chiều dọc
            int capHeight = 16;
            int valHeight = 32;
            int gap = 6;
            int totalContentH = capHeight + gap + valHeight;
            int topY = r.Y + Math.Max(8, (r.Height - totalContentH) / 2);
            int valY = topY + capHeight + gap;

            int barX = r.X + 16;
            int barW = 4;
            int textX = barX + barW + 8; // = r.X + 28: thẳng hàng hoàn hảo giữa tiêu đề và số
            int textW = Math.Max(20, r.Width - (textX - r.X) - 14);

            // Vạch màu nhỏ chỉ báo bên trái tiêu đề
            using (var path = SkinUtil.Round(new RectangleF(barX, topY + 1, barW, 14), 2))
            using (var br = new SolidBrush(_accent))
                g.FillPath(br, path);

            // Tiêu đề KPI (viết hoa, Muted, căn lề thẳng hàng với số bên dưới)
            using (var f = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, (_caption ?? "").ToUpper(), f,
                    new Rectangle(textX, topY, textW, capHeight),
                    ThemeManager.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Giá trị KPI (căn thẳng hàng với tiêu đề ở textX, cỡ chữ sắc nét, tự co vừa vặn)
            float fontSize = 18F;
            if (!string.IsNullOrEmpty(_value))
            {
                if (_value.Length > 16) fontSize = 13.5F;
                else if (_value.Length > 12) fontSize = 15F;
            }
            using (var f = new Font(ThemeManager.FontFamily, fontSize, FontStyle.Bold))
                TextRenderer.DrawText(g, _value ?? "", f,
                    new Rectangle(textX, valY, textW, valHeight),
                    ThemeManager.Ink,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}
