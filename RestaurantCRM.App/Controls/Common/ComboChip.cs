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
        private static readonly ToolTip SharedTip = new ToolTip();
        private readonly ComboBox _cb;
        private readonly string _caption;
        private bool _hover;
        private bool _compact;
        private bool _autoHide;

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

        public string Caption { get { return _caption; } }
        public ComboBox Combo { get { return _cb; } }

        /// <summary>Chip gọn: cao 30px, chỉ hiện giá trị (nhãn + giá trị đầy đủ đưa vào tooltip).</summary>
        public bool Compact
        {
            get { return _compact; }
            set
            {
                _compact = value;
                Height = value ? 30 : 38;
                Refit();
            }
        }

        /// <summary>Chỉ hiện khi bộ lọc đang bật (đã chọn khác mục đầu).</summary>
        public bool AutoHideWhenInactive
        {
            get { return _autoHide; }
            set { _autoHide = value; Refit(); }
        }

        private int _maxWidth;
        private bool _suppressed;

        /// <summary>Độ rộng tối đa do bố cục đặt (0 = mặc định). Chữ dài sẽ bị cắt "…".</summary>
        public int MaxWidth
        {
            get { return _maxWidth; }
            set { if (_maxWidth != value) { _maxWidth = value; Refit(); } }
        }

        /// <summary>Độ rộng chip cần để hiện đủ chữ (trong giới hạn mặc định).</summary>
        public int NaturalWidth { get; private set; }

        /// <summary>Bố cục thiếu chỗ: tạm ẩn chip dù bộ lọc đang bật (nút "Bộ lọc (n)" vẫn báo số lượng).</summary>
        public bool Suppressed
        {
            get { return _suppressed; }
            set { if (_suppressed != value) { _suppressed = value; Refit(); } }
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
                int w = (_compact ? 12 : 16) + 26;
                if (_caption.Length > 0 && !_compact) w += TextRenderer.MeasureText(_caption + ":", f1).Width;
                w += TextRenderer.MeasureText(Current() ?? "", f2).Width;
                NaturalWidth = _compact ? Math.Max(64, Math.Min(180, w)) : Math.Max(84, Math.Min(240, w));
                int target = _maxWidth > 0 ? Math.Min(NaturalWidth, _maxWidth) : NaturalWidth;
                if (Width != target) Width = target;
            }
            if (_compact)
            {
                string tip = _caption.Length > 0 ? _caption + ": " + Current() : Current();
                if (SharedTip.GetToolTip(this) != tip) SharedTip.SetToolTip(this, tip);
            }
            if (_autoHide) Visible = IsActive() && !_suppressed;
            Invalidate();
        }

        // Chiều rộng đã được đo theo pixel thật (Refit) → không để Windows phóng to thêm lần nữa
        // theo DPI, nếu không chip sẽ quá rộng và bị cắt mất chip phía sau.
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified & ~BoundsSpecified.Width);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            // Nếu đang lọc (SelectedIndex > 0) và người dùng bấm vào góc phải (nút ✕) -> đặt lại về mặc định
            if (IsActive() && e.X >= Width - 26 && _cb.Items.Count > 0)
            {
                _cb.SelectedIndex = 0;
                Refit();
                return;
            }

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
            int x = _compact ? 12 : 14;
            using (var f1 = new Font(ThemeManager.FontFamily, 8.5F))
            using (var f2 = new Font(ThemeManager.FontFamily, 8.5F, FontStyle.Bold))
            {
                if (_caption.Length > 0 && !_compact)
                {
                    string cap = _caption + ":";
                    TextRenderer.DrawText(g, cap, f1, new Rectangle(x, 0, Width, Height), ThemeManager.Muted,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    x += TextRenderer.MeasureText(g, cap, f1, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width + 4;
                }
                TextRenderer.DrawText(g, Current() ?? "", f2, new Rectangle(x, 0, Math.Max(10, Width - x - 24), Height), c,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
            if (active)
            {
                using (var fClear = new Font(ThemeManager.FontFamily, 7.5F, FontStyle.Bold))
                    TextRenderer.DrawText(g, "✕", fClear, new Rectangle(Width - 24, 0, 16, Height), ThemeManager.Primary,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
            else
            {
                using (var fi = ThemeManager.Icon(7F))
                    TextRenderer.DrawText(g, ThemeManager.Glyph.ChevronDown, fi, new Rectangle(Width - 24, 0, 14, Height), ThemeManager.Muted,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
        }
    }
}
