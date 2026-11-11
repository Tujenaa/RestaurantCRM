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
            MouseLeave += (s, e) => { if (_hoverClear || _hoverScope) { _hoverClear = _hoverScope = false; Cursor = Cursors.IBeam; Invalidate(); } };
            MouseDown += OnPillMouseDown;
            LayoutBox();
        }

        private Rectangle ClearRect => new Rectangle(Width - 30, 0, 26, Height);

        // ── Nút "Tìm theo" ở đầu ô (tùy chọn) ─────────────────────────────
        private string _scopeText;
        private Action<Point> _scopeClick;
        private bool _hoverScope;
        private int _scopeWidth;

        /// <summary>Bề rộng vùng nút "Tìm theo" (0 nếu không có).</summary>
        public int ScopeWidth { get { return _scopeWidth; } }

        private Rectangle ScopeRect => new Rectangle(0, 0, _scopeWidth, Height);

        /// <summary>Hiện nút nhỏ ở đầu ô (vd "Tên ▾"); bấm gọi onClick với điểm đặt menu (tọa độ của ô).</summary>
        public void SetScopeButton(string text, Action<Point> onClick)
        {
            _scopeText = text;
            _scopeClick = onClick;
            using (var f = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
                _scopeWidth = string.IsNullOrEmpty(text) ? 0 : TextRenderer.MeasureText(text + " ▾", f, Size.Empty, TextFormatFlags.NoPadding).Width + 24;
            LayoutBox();
            Invalidate();
        }

        private void OnPillMouseMove(object sender, MouseEventArgs e)
        {
            bool overScope = _scopeWidth > 0 && ScopeRect.Contains(e.Location);
            if (overScope != _hoverScope) { _hoverScope = overScope; Invalidate(); }

            bool hasText = _tb != null && _tb.TextLength > 0;
            bool overClear = hasText && ClearRect.Contains(e.Location);
            if (overClear != _hoverClear)
            {
                _hoverClear = overClear;
                Invalidate();
            }
            Cursor = (_hoverClear || overScope) ? Cursors.Hand : Cursors.IBeam;
        }

        private void OnPillMouseDown(object sender, MouseEventArgs e)
        {
            if (_scopeWidth > 0 && _scopeClick != null && ScopeRect.Contains(e.Location))
            {
                _scopeClick(new Point(0, Height + 2));
                return;
            }
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
            int left = _scopeWidth > 0 ? _scopeWidth + 8 : 14;
            _tb.SetBounds(left, (Height - _tb.PreferredHeight) / 2 + 1, Math.Max(20, Width - left - rightPad), _tb.PreferredHeight);
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

            if (_scopeWidth > 0)
            {
                var sr = ScopeRect;
                if (_hoverScope)
                    using (var path = SkinUtil.Round(new RectangleF(3.5f, 3.5f, sr.Width - 6f, Height - 8f), (Height - 8f) / 2))
                    using (var br = new SolidBrush(ThemeManager.PrimarySoft)) g.FillPath(br, path);
                using (var f = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
                    TextRenderer.DrawText(g, _scopeText + " ▾", f, new Rectangle(12, 0, sr.Width - 14, Height), ThemeManager.Primary,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                using (var pen = new Pen(ThemeManager.Line))
                    g.DrawLine(pen, sr.Right, 9, sr.Right, Height - 9);
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
