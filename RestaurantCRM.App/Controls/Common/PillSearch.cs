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
        private bool _hoverClear;

        public PillSearch(TextBox tb, string placeholder)
        {
            _tb = tb;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 38;
            Width = 280;
            if (tb.Parent != null) tb.Parent.Controls.Remove(tb);
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = ThemeManager.Surface2;
            tb.ForeColor = ThemeManager.Ink;
            tb.Font = new Font(ThemeManager.FontFamily, 9.5F);
            tb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            Controls.Add(tb);
            tb.Enter += (s, e) => { _focus = true; Invalidate(); };
            tb.Leave += (s, e) => { _focus = false; Invalidate(); };
            tb.TextChanged += (s, e) => { LayoutBox(); Invalidate(); };
            if (!string.IsNullOrEmpty(placeholder)) UiStyler.SetPlaceholder(tb, placeholder);
            Cursor = Cursors.IBeam;
            MouseMove += OnPillMouseMove;
            MouseLeave += (s, e) => { if (_hoverClear) { _hoverClear = false; Cursor = Cursors.IBeam; Invalidate(); } };
            MouseDown += OnPillMouseDown;
            LayoutBox();
        }

        private Rectangle ClearRect => new Rectangle(Width - 30, 0, 26, Height);

        private void OnPillMouseMove(object sender, MouseEventArgs e)
        {
            bool hasText = _tb != null && _tb.TextLength > 0;
            bool overClear = hasText && ClearRect.Contains(e.Location);
            if (overClear != _hoverClear)
            {
                _hoverClear = overClear;
                Cursor = _hoverClear ? Cursors.Hand : Cursors.IBeam;
                Invalidate();
            }
        }

        private void OnPillMouseDown(object sender, MouseEventArgs e)
        {
            if (_tb != null && _tb.TextLength > 0 && ClearRect.Contains(e.Location))
            {
                _tb.Clear();
                _tb.Focus();
                _hoverClear = false;
                Cursor = Cursors.IBeam;
                Invalidate();
                return;
            }
            _tb?.Focus();
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); LayoutBox(); }
        private void LayoutBox()
        {
            if (_tb == null) return;
            int rightPad = _tb.TextLength > 0 ? 32 : 14;
            _tb.SetBounds(14, (Height - _tb.PreferredHeight) / 2 + 1, Math.Max(20, Width - 14 - rightPad), _tb.PreferredHeight);
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

            if (_tb != null && _tb.TextLength > 0)
            {
                using (var fClear = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
                {
                    Color clearColor = _hoverClear ? ThemeManager.Red : ThemeManager.Muted;
                    TextRenderer.DrawText(g, "✕", fClear, ClearRect, clearColor,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
                }
            }
        }
    }
}
