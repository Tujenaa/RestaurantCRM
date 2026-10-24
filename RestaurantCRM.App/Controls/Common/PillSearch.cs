using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    // ───────────────────────────────────────────────────────────────────────
    //  Ô TÌM KIẾM DẠNG PILL
    // ───────────────────────────────────────────────────────────────────────
    public class PillSearch : Control
    {
        private readonly TextBox _tb;
        private bool _focus;

        public PillSearch(TextBox tb, string placeholder)
        {
            _tb = tb;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 38;
            Width = 300;
            if (tb.Parent != null) tb.Parent.Controls.Remove(tb);
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = ThemeManager.Surface2;
            tb.ForeColor = ThemeManager.Ink;
            tb.Font = new Font(ThemeManager.FontFamily, 9.5F);
            tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            Controls.Add(tb);
            tb.Enter += (s, e) => { _focus = true; Invalidate(); };
            tb.Leave += (s, e) => { _focus = false; Invalidate(); };
            if (!string.IsNullOrEmpty(placeholder)) UiStyler.SetPlaceholder(tb, placeholder);
            Cursor = Cursors.IBeam;
            Click += (s, e) => tb.Focus();
            LayoutBox();
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); LayoutBox(); }
        private void LayoutBox()
        {
            if (_tb == null) return;
            _tb.SetBounds(38, (Height - _tb.PreferredHeight) / 2 + 1, Math.Max(20, Width - 38 - 14), _tb.PreferredHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(SkinUtil.Backdrop(this));
            var r = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = SkinUtil.Round(r, r.Height / 2))
            {
                using (var br = new SolidBrush(ThemeManager.Surface2)) g.FillPath(br, path);
                using (var pen = new Pen(_focus ? ThemeManager.Primary : ThemeManager.Line, _focus ? 1.6f : 1f)) g.DrawPath(pen, path);
            }
            using (var f = ThemeManager.Icon(9.5F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.Search, f, new Rectangle(13, 0, 18, Height), _focus ? ThemeManager.Primary : ThemeManager.Muted,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
        }
    }
}
