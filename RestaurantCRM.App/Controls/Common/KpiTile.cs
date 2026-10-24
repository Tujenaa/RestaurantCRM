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
            // vạch màu nhỏ bên trái
            using (var path = SkinUtil.Round(new RectangleF(r.X + 16, r.Y + 18, 4, 16), 2))
            using (var br = new SolidBrush(_accent)) g.FillPath(br, path);
            using (var f = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, (_caption ?? "").ToUpper(), f, new Rectangle(r.X + 28, r.Y + 18, r.Width - 40, 16), ThemeManager.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            using (var f = new Font(ThemeManager.FontFamily, 17F, FontStyle.Bold))
                TextRenderer.DrawText(g, _value ?? "", f, new Rectangle(r.X + 15, r.Y + 40, r.Width - 26, 34), ThemeManager.Ink,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }
}
