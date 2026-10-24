using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    // ───────────────────────────────────────────────────────────────────────
    //  CHIP LỌC (thay giao diện ComboBox, ComboBox gốc vẫn giữ dữ liệu & sự kiện)
    // ───────────────────────────────────────────────────────────────────────
    public class ComboChip : Control
    {
        private readonly ComboBox _cb;
        private readonly string _caption;
        private bool _hover;

        public ComboChip(ComboBox cb, string caption)
        {
            _cb = cb;
            _caption = SkinUtil.CleanText((caption ?? "").TrimEnd(':', ' '));
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 38;
            Cursor = Cursors.Hand;
            cb.SelectedIndexChanged += (s, e) => Refit();
            cb.DataSourceChanged += (s, e) => Refit();
            cb.TextChanged += (s, e) => Refit();
            Refit();
        }

        private string Current()
        {
            if (_cb.SelectedIndex >= 0 && _cb.SelectedIndex < _cb.Items.Count) return _cb.GetItemText(_cb.Items[_cb.SelectedIndex]);
            return _cb.Text;
        }

        private bool IsActive() { return _cb.SelectedIndex > 0; }

        public void Refit()
        {
            using (var f1 = new Font(ThemeManager.FontFamily, 8.5F))
            using (var f2 = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
            {
                int w = 40 + 30;
                if (_caption.Length > 0) w += TextRenderer.MeasureText(_caption + ":", f1).Width;
                w += TextRenderer.MeasureText(Current() ?? "", f2).Width;
                Width = Math.Min(320, w);
            }
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            var menu = new ContextMenuStrip { ShowImageMargin = false, Font = new Font(ThemeManager.FontFamily, 9.5F), BackColor = Color.White };
            for (int i = 0; i < _cb.Items.Count; i++)
            {
                int idx = i;
                bool sel = idx == _cb.SelectedIndex;
                var it = new ToolStripMenuItem((sel ? "✓  " : "     ") + _cb.GetItemText(_cb.Items[i])) { ForeColor = sel ? ThemeManager.Primary : ThemeManager.Ink2 };
                it.Click += (s, ev) => { _cb.SelectedIndex = idx; Refit(); };
                menu.Items.Add(it);
            }
            menu.Closed += (s, ev) => BeginInvoke((Action)(() => menu.Dispose()));
            menu.Show(this, new Point(0, Height + 4));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(SkinUtil.Backdrop(this));
            bool active = IsActive();
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = SkinUtil.Round(r, r.Height / 2))
            {
                Color fill = active ? ThemeManager.PrimarySoft : (_hover ? ThemeManager.Surface2 : Color.White);
                using (var br = new SolidBrush(fill)) g.FillPath(br, path);
                using (var pen = new Pen(active ? Color.FromArgb(110, ThemeManager.Primary) : ThemeManager.Line)) g.DrawPath(pen, path);
            }
            Color c = active ? ThemeManager.Primary : ThemeManager.Ink2;
            using (var fi = ThemeManager.Icon(8.5F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.Filter, fi, new Rectangle(13, 0, 16, Height), c,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            int x = 36;
            using (var f1 = new Font(ThemeManager.FontFamily, 8.5F))
            using (var f2 = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
            {
                if (_caption.Length > 0)
                {
                    string cap = _caption + ":";
                    TextRenderer.DrawText(g, cap, f1, new Rectangle(x, 0, Width, Height), ThemeManager.Muted,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    x += TextRenderer.MeasureText(g, cap, f1, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width + 5;
                }
                TextRenderer.DrawText(g, Current() ?? "", f2, new Rectangle(x, 0, Width - x - 28, Height), c,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            using (var fi = ThemeManager.Icon(7F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.ChevronDown, fi, new Rectangle(Width - 26, 0, 14, Height), ThemeManager.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
        }
    }
}
