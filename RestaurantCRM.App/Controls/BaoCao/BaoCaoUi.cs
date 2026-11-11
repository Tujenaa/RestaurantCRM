using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace RestaurantCRM.AdminApp.Controls.BaoCao
{
    // ═══════════════════════════════════════════════════════════════════════
    //  BỘ CONTROL TỰ VẼ CHO TRANG "BÁO CÁO DOANH THU"
    //  Màu sắc, cỡ chữ (theo pixel để không bị phình to/đè chữ khi DPI 125–150%),
    //  hỗ trợ tooltip hiện số tiền/nội dung đầy đủ khi rê chuột vào ô bị "..."
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Bảng màu, font và các hàm định dạng dùng chung cho trang báo cáo.</summary>
    public static class BcTheme
    {
        public static readonly Color Brand = Hex("#C2410C");
        public static readonly Color Brand2 = Hex("#EA7A3B");
        public static readonly Color BrandSoft = Hex("#FDEEE4");
        public static readonly Color BrandBorder = Hex("#F7C9AA");
        public static readonly Color Amber = Hex("#E59E2B");
        public static readonly Color AmberSoft = Hex("#FCF1DE");
        public static readonly Color AmberText = Hex("#A16207");
        public static readonly Color Bg = Hex("#FAF6F2");
        public static readonly Color Card = Color.White;
        public static readonly Color Line = Hex("#F0E6DD");
        public static readonly Color Line2 = Hex("#F6EFE9");
        public static readonly Color Text = Hex("#2A1C13");
        public static readonly Color Text2 = Hex("#5C4A3D");
        public static readonly Color Muted = Hex("#9A8677");
        public static readonly Color Axis = Hex("#A8968A");
        public static readonly Color Faded = Hex("#B9A99C");
        public static readonly Color Zero = Hex("#C4B5A9");
        public static readonly Color Green = Hex("#15803D");
        public static readonly Color GreenSoft = Hex("#E7F6EC");
        public static readonly Color Red = Hex("#DC2626");
        public static readonly Color RedSoft = Hex("#FDECEC");
        public static readonly Color HeaderBg = Hex("#FCF9F6");
        public static readonly Color TotalBg = Hex("#FCF6F1");
        public static readonly Color HoverBg = Hex("#FFFAF6");
        public static readonly Color Grid = Hex("#F1E9E2");
        public static readonly Color TagBg = Hex("#F4EEE9");
        public static readonly Color SegBg = Hex("#FBF8F5");
        public static readonly Color MiniBg = Hex("#FCF8F4");
        public static readonly Color TrackBg = Hex("#F3ECE6");
        public static readonly Color EmptyBar = Hex("#FAF5F1");
        public static readonly Color Rank3 = Hex("#F3B48A");

        /// <summary>4 sắc độ cam cho Quý 1 → Quý 4.</summary>
        public static readonly Color[] QuarterColors = { Hex("#7C2D12"), Hex("#C2410C"), Hex("#EA7A3B"), Hex("#F6B98F") };
        /// <summary>3 sắc độ cho 3 năm liên tiếp (năm cũ nhạt → năm mới đậm).</summary>
        public static readonly Color[] YearColors = { Hex("#F6B98F"), Hex("#EA7A3B"), Hex("#C2410C") };
        public static readonly string[] QuarterNames = { "Quý 1", "Quý 2", "Quý 3", "Quý 4" };

        public static readonly CultureInfo Num = CultureInfo.InvariantCulture;
        public const string Minus = "\u2212";
        public const string EnDash = "\u2013";

        private static Color Hex(string h) { return ColorTranslator.FromHtml(h); }

        // ---------- Font (GraphicsUnit.Pixel để kích thước chữ cố định, gọn gàng ở mọi mức DPI) ----------
        private static string _family;
        /// <summary>Be Vietnam Pro nếu máy có cài, dự phòng Inter / Segoe UI.</summary>
        public static string Family
        {
            get
            {
                if (_family != null) return _family;
                _family = "Segoe UI";
                try
                {
                    using (var fc = new InstalledFontCollection())
                    {
                        foreach (var name in new[] { "Be Vietnam Pro", "Inter" })
                        {
                            if (fc.Families.Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)))
                            {
                                _family = name;
                                break;
                            }
                        }
                    }
                }
                catch { }
                return _family;
            }
        }

        private static readonly Dictionary<string, Font> _fonts = new Dictionary<string, Font>();
        /// <summary>Lấy font theo đơn vị Pixel (tỷ lệ ~1.22x từ cỡ quy chiếu) giúp chữ gọn, không bao giờ đè nhau.</summary>
        public static Font F(float size, FontStyle style = FontStyle.Regular)
        {
            float px = (float)Math.Round(size * 1.22f, 1);
            string key = px.ToString(Num) + "|" + (int)style;
            Font f;
            if (!_fonts.TryGetValue(key, out f))
            {
                f = new Font(Family, px, style, GraphicsUnit.Pixel);
                _fonts[key] = f;
            }
            return f;
        }

        // ---------- Định dạng ----------
        public static string Money(double v) { return Math.Round(v).ToString("#,##0", Num) + " đ"; }
        public static string SignedMoney(double v) { return (v >= 0 ? "+" : Minus) + Money(Math.Abs(v)); }
        public static string Pct(double v) { return v.ToString("0.0", Num) + "%"; }
        public static string SignedPct(double v) { return (v >= 0 ? "+" : Minus) + Pct(Math.Abs(v)); }
        /// <summary>Định dạng triệu đồng gọn: 4.3 tr, 25 tr.</summary>
        public static string Tr(double v) { return (v / 1e6).ToString("0.##", Num) + " tr"; }
        public static string MoneyAxis(double v) { return v == 0 ? "0" : Tr(v); }
        public static string Qty(double v) { return Math.Round(v).ToString("#,##0", Num); }

        /// <summary>Bỏ emoji / ký hiệu icon trong chuỗi lấy từ API (trang chỉ dùng chữ).</summary>
        public static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (char.IsSurrogate(c) || c == '\uFE0F' || c == '\u200D') continue;
                if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherSymbol) continue;
                sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        /// <summary>Viết hoa chữ cái đầu: "bán hàng" → "Bán hàng".</summary>
        public static string Cap(string s)
        {
            s = Clean(s);
            if (s.Length == 0) return s;
            return char.ToUpper(s[0], new CultureInfo("vi-VN")) + s.Substring(1);
        }

        /// <summary>Bước chia trục "đẹp" (1, 2, 2.5, 5 × 10^k).</summary>
        public static double NiceStep(double max, int ticks)
        {
            if (max <= 0 || double.IsNaN(max)) return 1;
            double raw = max / Math.Max(1, ticks);
            double mag = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            double n = raw / mag;
            double s = n <= 1 ? 1 : n <= 2 ? 2 : n <= 2.5 ? 2.5 : n <= 5 ? 5 : 10;
            return s * mag;
        }

        // ---------- Vẽ ----------
        public const TextFormatFlags BaseFlags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        public static GraphicsPath Round(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = Math.Min(rad * 2, Math.Min(r.Width, r.Height));
            if (d <= 0.5f) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static void FillRound(Graphics g, RectangleF r, float rad, Color fill, Color? border = null, float borderWidth = 1f)
        {
            if (r.Width <= 0 || r.Height <= 0) return;
            using (var path = Round(r, rad))
            {
                using (var br = new SolidBrush(fill)) g.FillPath(br, path);
                if (border.HasValue)
                    using (var pen = new Pen(border.Value, borderWidth)) g.DrawPath(pen, path);
            }
        }

        /// <summary>Bóng đổ mềm rất nhẹ cho tooltip.</summary>
        public static void SoftShadow(Graphics g, RectangleF r, float rad)
        {
            for (int k = 6; k >= 1; k--)
            {
                var rr = new RectangleF(r.X - k, r.Y - k + 4, r.Width + k * 2, r.Height + k * 2);
                using (var path = Round(rr, rad + k))
                using (var br = new SolidBrush(Color.FromArgb(6, 90, 42, 10)))
                    g.FillPath(br, path);
            }
        }

        public static void DrawText(Graphics g, string s, Font f, Rectangle r, Color c, TextFormatFlags extra = TextFormatFlags.Default)
        {
            TextRenderer.DrawText(g, s ?? "", f, r, c, BaseFlags | TextFormatFlags.VerticalCenter | extra);
        }

        public static int Measure(string s, Font f)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            return TextRenderer.MeasureText(s, f, new Size(int.MaxValue, int.MaxValue), BaseFlags).Width;
        }

        /// <summary>Màu nền thực của control cha (bỏ qua nền trong suốt).</summary>
        public static Color BackOf(Control c)
        {
            for (var p = c.Parent; p != null; p = p.Parent)
            {
                var rp = p as RoundedPanel;
                if (rp != null) return rp.FillColor;
                if (p.BackColor.A == 255) return p.BackColor;
            }
            return Bg;
        }
    }

    /// <summary>Control tự vẽ, có double-buffer.</summary>
    public abstract class BcControl : Control
    {
        protected BcControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw, true);
            Font = BcTheme.F(9F);
        }

        protected Color Backdrop { get { return BcTheme.BackOf(this); } }

        protected Graphics Prepare(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Backdrop);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  KHUNG (CARD): nền trắng, bo 16px, viền 1px, tiêu đề + dòng phụ
    // ───────────────────────────────────────────────────────────────────────
    public class BcCard : RoundedPanel
    {
        private string _title, _subtitle;
        /// <summary>Khoảng chừa bên phải tiêu đề (để không đè lên legend/nút góc phải).</summary>
        public int HeaderRightReserve { get; set; }

        public BcCard()
        {
            Radius = 16;
            Shadow = 0;
            FillColor = BcTheme.Card;
            BorderColor = BcTheme.Line;
        }

        public string Title { get { return _title; } set { if (_title == value) return; _title = value; Invalidate(); } }
        public string Subtitle { get { return _subtitle; } set { if (_subtitle == value) return; _subtitle = value; Invalidate(); } }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (string.IsNullOrEmpty(_title)) return;
            var g = e.Graphics;
            int availW = Math.Max(40, Width - 44 - HeaderRightReserve);
            BcTheme.DrawText(g, _title, BcTheme.F(12F, FontStyle.Bold), new Rectangle(22, 16, availW, 24), BcTheme.Text, TextFormatFlags.EndEllipsis);
            if (!string.IsNullOrEmpty(_subtitle))
                BcTheme.DrawText(g, _subtitle, BcTheme.F(9F), new Rectangle(22, 41, Math.Max(40, Width - 44), 18), BcTheme.Muted, TextFormatFlags.EndEllipsis);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  THANH TAB: tab đang chọn nền brand, chữ trắng
    // ───────────────────────────────────────────────────────────────────────
    public class BcTabBar : BcControl
    {
        private string[] _items = new string[0];
        private Rectangle[] _rects = new Rectangle[0];
        private int _sel, _hover = -1;

        public event EventHandler SelectedIndexChanged;

        public BcTabBar() { Height = 50; Cursor = Cursors.Hand; }

        public string[] Items { get { return _items; } set { _items = value ?? new string[0]; Invalidate(); } }

        public int SelectedIndex
        {
            get { return _sel; }
            set
            {
                if (value == _sel || value < 0 || value >= _items.Length) return;
                _sel = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Calc()
        {
            var f = BcTheme.F(10F, FontStyle.Bold);
            _rects = new Rectangle[_items.Length];
            int x = 6;
            for (int i = 0; i < _items.Length; i++)
            {
                int w = BcTheme.Measure(_items[i], f) + 40;
                _rects[i] = new Rectangle(x, 6, w, Height - 12);
                x += w + 4;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            BcTheme.FillRound(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 14, BcTheme.Card, BcTheme.Line);
            Calc();
            var f = BcTheme.F(10F, FontStyle.Bold);
            for (int i = 0; i < _items.Length; i++)
            {
                var r = _rects[i];
                Color fc = BcTheme.Text2;
                if (i == _sel) { BcTheme.FillRound(g, r, 10, BcTheme.Brand); fc = Color.White; }
                else if (i == _hover) BcTheme.FillRound(g, r, 10, BcTheme.SegBg);
                BcTheme.DrawText(g, _items[i], f, r, fc, TextFormatFlags.HorizontalCenter);
            }
        }

        private int Hit(Point p) { for (int i = 0; i < _rects.Length; i++) if (_rects[i].Contains(p)) return i; return -1; }
        protected override void OnMouseMove(MouseEventArgs e) { int h = Hit(e.Location); if (h != _hover) { _hover = h; Invalidate(); } base.OnMouseMove(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseClick(MouseEventArgs e) { int h = Hit(e.Location); if (h >= 0) SelectedIndex = h; base.OnMouseClick(e); }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  NÚT CHUYỂN (SEGMENTED): "Xem bảng | Xem biểu đồ", "4 quý | 3 năm"
    // ───────────────────────────────────────────────────────────────────────
    public class BcSegmented : BcControl
    {
        private string[] _items = new string[0];
        private Rectangle[] _rects = new Rectangle[0];
        private int _sel, _hover = -1;

        public event EventHandler SelectedIndexChanged;

        public BcSegmented(params string[] items)
        {
            Height = 34;
            Cursor = Cursors.Hand;
            Items = items;
        }

        public string[] Items
        {
            get { return _items; }
            set { _items = value ?? new string[0]; Calc(); Invalidate(); }
        }

        public int SelectedIndex
        {
            get { return _sel; }
            set
            {
                if (value == _sel || value < 0 || value >= _items.Length) return;
                _sel = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Calc()
        {
            var f = BcTheme.F(9.25F, FontStyle.Bold);
            _rects = new Rectangle[_items.Length];
            int x = 3;
            for (int i = 0; i < _items.Length; i++)
            {
                int w = BcTheme.Measure(_items[i], f) + 24;
                _rects[i] = new Rectangle(x, 3, w, Height - 6);
                x += w;
            }
            Width = x + 3;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            BcTheme.FillRound(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10, BcTheme.SegBg, BcTheme.Line);
            var f = BcTheme.F(9.25F, FontStyle.Bold);
            for (int i = 0; i < _items.Length; i++)
            {
                var r = _rects[i];
                Color fc = i == _hover ? BcTheme.Text : BcTheme.Text2;
                if (i == _sel)
                {
                    BcTheme.FillRound(g, new RectangleF(r.X, r.Y + 1, r.Width, r.Height), 8, Color.FromArgb(30, 80, 40, 10));
                    BcTheme.FillRound(g, r, 8, Color.White);
                    fc = BcTheme.Brand;
                }
                BcTheme.DrawText(g, _items[i], f, r, fc, TextFormatFlags.HorizontalCenter);
            }
        }

        private int Hit(Point p) { for (int i = 0; i < _rects.Length; i++) if (_rects[i].Contains(p)) return i; return -1; }
        protected override void OnMouseMove(MouseEventArgs e) { int h = Hit(e.Location); if (h != _hover) { _hover = h; Invalidate(); } base.OnMouseMove(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseClick(MouseEventArgs e) { int h = Hit(e.Location); if (h >= 0) SelectedIndex = h; base.OnMouseClick(e); }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  CHIP CHỌN: "Cả năm | Quý 1 …", "Theo doanh thu | Theo số lượng bán"
    // ───────────────────────────────────────────────────────────────────────
    public class BcChips : BcControl
    {
        private string[] _items = new string[0];
        private Rectangle[] _rects = new Rectangle[0];
        private int _sel, _hover = -1;

        public event EventHandler SelectedIndexChanged;

        public BcChips(params string[] items)
        {
            Height = 30;
            Cursor = Cursors.Hand;
            Items = items;
        }

        public string[] Items
        {
            get { return _items; }
            set { _items = value ?? new string[0]; Calc(); Invalidate(); }
        }

        public int SelectedIndex
        {
            get { return _sel; }
            set
            {
                if (value == _sel || value < 0 || value >= _items.Length) return;
                _sel = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Calc()
        {
            var f = BcTheme.F(9F, FontStyle.Bold);
            _rects = new Rectangle[_items.Length];
            int x = 0;
            for (int i = 0; i < _items.Length; i++)
            {
                int w = BcTheme.Measure(_items[i], f) + 24;
                _rects[i] = new Rectangle(x, 1, w, Height - 2);
                x += w + 6;
            }
            Width = Math.Max(10, x - 6 + 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            var f = BcTheme.F(9F, FontStyle.Bold);
            for (int i = 0; i < _items.Length; i++)
            {
                var r = new RectangleF(_rects[i].X + 0.5f, _rects[i].Y + 0.5f, _rects[i].Width - 1, _rects[i].Height - 1);
                bool on = i == _sel;
                Color fill = on ? BcTheme.BrandSoft : (i == _hover ? BcTheme.SegBg : Color.White);
                BcTheme.FillRound(g, r, r.Height / 2, fill, on ? BcTheme.BrandBorder : BcTheme.Line);
                BcTheme.DrawText(g, _items[i], f, _rects[i], on ? BcTheme.Brand : BcTheme.Text2, TextFormatFlags.HorizontalCenter);
            }
        }

        private int Hit(Point p) { for (int i = 0; i < _rects.Length; i++) if (_rects[i].Contains(p)) return i; return -1; }
        protected override void OnMouseMove(MouseEventArgs e) { int h = Hit(e.Location); if (h != _hover) { _hover = h; Invalidate(); } base.OnMouseMove(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseClick(MouseEventArgs e) { int h = Hit(e.Location); if (h >= 0) SelectedIndex = h; base.OnMouseClick(e); }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Ô CHỌN (SELECT): "2026 ▾", "Chức vụ: Tất cả ▾"
    // ───────────────────────────────────────────────────────────────────────
    public class BcSelect : BcControl
    {
        private List<string> _items = new List<string>();
        private int _sel = -1;
        private bool _hover;
        private string _caption;

        public event EventHandler SelectedIndexChanged;

        /// <summary>true: giá trị in đậm màu chữ chính (ô chọn năm); false: chữ phụ (bộ lọc).</summary>
        public bool BoldValue { get; set; }

        public BcSelect(string caption = null)
        {
            _caption = caption;
            Height = 34;
            Cursor = Cursors.Hand;
            Refit();
        }

        public string SelectedItem { get { return _sel >= 0 && _sel < _items.Count ? _items[_sel] : null; } }
        public IList<string> Items { get { return _items.AsReadOnly(); } }

        public int SelectedIndex
        {
            get { return _sel; }
            set
            {
                if (value == _sel || value < 0 || value >= _items.Count) return;
                _sel = value;
                Refit();
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Nạp danh sách mà không phát sự kiện.</summary>
        public void SetItems(IEnumerable<string> items, int selected)
        {
            _items = (items ?? Enumerable.Empty<string>()).ToList();
            _sel = _items.Count == 0 ? -1 : Math.Max(0, Math.Min(selected, _items.Count - 1));
            Refit();
            Invalidate();
        }

        private string CaptionText { get { return string.IsNullOrEmpty(_caption) ? "" : _caption + ": "; } }
        private Font ValueFont { get { return BoldValue ? BcTheme.F(9.5F, FontStyle.Bold) : BcTheme.F(9.25F); } }

        public void Refit()
        {
            int w = 12 + 32;
            if (CaptionText.Length > 0) w += BcTheme.Measure(CaptionText, BcTheme.F(9.25F));
            w += BcTheme.Measure(SelectedItem ?? "", ValueFont);
            Width = Math.Max(78, w);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            BcTheme.FillRound(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10, _hover ? BcTheme.SegBg : Color.White, BcTheme.Line);
            int x = 12;
            if (CaptionText.Length > 0)
            {
                var fc = BcTheme.F(9.25F);
                BcTheme.DrawText(g, CaptionText, fc, new Rectangle(x, 0, Width, Height), BcTheme.Text2);
                x += BcTheme.Measure(CaptionText, fc);
            }
            BcTheme.DrawText(g, SelectedItem ?? "", ValueFont, new Rectangle(x, 0, Math.Max(10, Width - x - 26), Height),
                BoldValue ? BcTheme.Text : BcTheme.Text2, TextFormatFlags.EndEllipsis);

            float cx = Width - 17, cy = Height / 2f;
            using (var pen = new Pen(BcTheme.Text2, 1.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                g.DrawLines(pen, new[] { new PointF(cx - 4, cy - 2), new PointF(cx, cy + 2), new PointF(cx + 4, cy - 2) });
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (_items.Count == 0) return;
            var menu = new ContextMenuStrip { ShowImageMargin = false, ShowCheckMargin = false, Font = BcTheme.F(9.25F), BackColor = Color.White };
            for (int i = 0; i < _items.Count; i++)
            {
                int idx = i;
                var it = new ToolStripMenuItem(_items[i])
                {
                    ForeColor = idx == _sel ? BcTheme.Brand : BcTheme.Text2,
                    Font = idx == _sel ? BcTheme.F(9.25F, FontStyle.Bold) : BcTheme.F(9.25F),
                    Padding = new Padding(4, 3, 4, 3)
                };
                it.Click += (s, ev) => SelectedIndex = idx;
                menu.Items.Add(it);
            }
            menu.Closed += (s, ev) => BeginInvoke((Action)(() => menu.Dispose()));
            menu.Show(this, new Point(0, Height + 4));
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  NÚT VIỀN: "Làm mới"
    // ───────────────────────────────────────────────────────────────────────
    public class BcButton : BcControl
    {
        private bool _hover, _down;

        public BcButton(string text)
        {
            Text = text;
            Height = 34;
            Cursor = Cursors.Hand;
            Width = BcTheme.Measure(text, BcTheme.F(9.25F, FontStyle.Bold)) + 30;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            Color fill = !Enabled ? BcTheme.SegBg : _down ? BcTheme.BrandSoft : _hover ? BcTheme.SegBg : Color.White;
            BcTheme.FillRound(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10, fill, _hover && Enabled ? BcTheme.BrandBorder : BcTheme.Line);
            BcTheme.DrawText(g, Text, BcTheme.F(9.25F, FontStyle.Bold), ClientRectangle,
                Enabled ? (_hover ? BcTheme.Brand : BcTheme.Text2) : BcTheme.Faded, TextFormatFlags.HorizontalCenter);
        }

        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Ô TÌM KIẾM (không icon, có chữ gợi ý)
    // ───────────────────────────────────────────────────────────────────────
    public class BcSearchBox : BcControl
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private readonly string _placeholder;
        public readonly TextBox Box;

        public event EventHandler SearchChanged;

        public BcSearchBox(string placeholder)
        {
            _placeholder = placeholder;
            Cursor = Cursors.IBeam;
            Box = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = BcTheme.F(9.25F),
                ForeColor = BcTheme.Text,
                BackColor = Color.White
            };
            Box.HandleCreated += (s, e) => SendMessage(Box.Handle, EM_SETCUEBANNER, (IntPtr)1, _placeholder);
            Box.TextChanged += (s, e) => SearchChanged?.Invoke(this, EventArgs.Empty);
            Box.GotFocus += (s, e) => Invalidate();
            Box.LostFocus += (s, e) => Invalidate();
            Controls.Add(Box);
            Height = 34;
        }

        public string Value { get { return Box.Text; } set { Box.Text = value ?? ""; } }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (Box == null) return;
            int h = Box.PreferredHeight;
            Box.SetBounds(12, (Height - h) / 2, Math.Max(20, Width - 24), h);
        }

        protected override void OnClick(EventArgs e) { Box?.Focus(); base.OnClick(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            BcTheme.FillRound(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 10, Color.White,
                Box != null && Box.Focused ? BcTheme.BrandBorder : BcTheme.Line);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  CHÚ THÍCH (LEGEND)
    // ───────────────────────────────────────────────────────────────────────
    public class BcLegend : BcControl
    {
        private string[] _labels = new string[0];
        private Color[] _colors = new Color[0];

        public BcLegend() { Height = 20; }

        public void SetItems(string[] labels, Color[] colors)
        {
            _labels = labels ?? new string[0];
            _colors = colors ?? new Color[0];
            var f = BcTheme.F(8.75F);
            int w = 0;
            foreach (var l in _labels) w += 14 + BcTheme.Measure(l, f) + 14;
            Width = Math.Max(10, w - 14 + 2);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            var f = BcTheme.F(8.75F);
            int x = 0;
            for (int i = 0; i < _labels.Length; i++)
            {
                Color c = i < _colors.Length ? _colors[i] : BcTheme.Brand;
                BcTheme.FillRound(g, new RectangleF(x, Height / 2f - 4.5f, 9, 9), 2.5f, c);
                x += 14;
                int tw = BcTheme.Measure(_labels[i], f);
                BcTheme.DrawText(g, _labels[i], f, new Rectangle(x, 0, tw + 2, Height), BcTheme.Text2);
                x += tw + 14;
            }
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  Ô TÓM TẮT NHỎ: "Quý cao nhất / Quý lỗ / Tăng trưởng"
    // ───────────────────────────────────────────────────────────────────────
    public class BcMiniStat : BcControl
    {
        private static readonly ToolTip s_tip = new ToolTip { InitialDelay = 120, ReshowDelay = 80 };
        private string _label = "", _value = "";
        private Color _valueColor = BcTheme.Text;

        public BcMiniStat() { Height = 60; }

        public void SetValue(string label, string value, Color? color = null)
        {
            _label = label ?? "";
            _value = value ?? "";
            _valueColor = color ?? BcTheme.Text;
            s_tip.SetToolTip(this, string.IsNullOrEmpty(_label) ? _value : (_label + ": " + _value));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            BcTheme.FillRound(g, new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f), 12, BcTheme.MiniBg, BcTheme.Line);
            BcTheme.DrawText(g, _label, BcTheme.F(8.5F), new Rectangle(12, 8, Width - 24, 18), BcTheme.Muted, TextFormatFlags.EndEllipsis);
            BcTheme.DrawText(g, _value, BcTheme.F(11F, FontStyle.Bold), new Rectangle(12, 28, Width - 24, 24), _valueColor, TextFormatFlags.EndEllipsis);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  BẢNG TỰ VẼ: chữ nhỏ gọn, ưu tiên không cắt cột số tiền,
    //  rê chuột vào chỗ bị "..." hoặc ô tiền sẽ hiện tooltip đầy đủ.
    // ───────────────────────────────────────────────────────────────────────
    public class BcColumn
    {
        public string Header;
        public bool Right;
        public float Weight;
        /// <summary>Cho phép thu hẹp cột này trước khi màn hình hẹp (áp dụng cho cột chữ dài như Tên món / Danh mục).</summary>
        public bool Shrinkable;
        public BcColumn(string header, bool right = false, float weight = 1f, bool shrinkable = false)
        {
            Header = header;
            Right = right;
            Weight = weight;
            Shrinkable = shrinkable;
        }
    }

    public enum BcCellKind { Text, Rank, Tag, Share }

    public class BcCell
    {
        public string Text;
        public BcCellKind Kind;
        public Color? Fore;
        public Color Back;
        public bool Bold;
        public double Ratio;
        public bool IsMoney;

        public static BcCell T(string text, Color? fore = null, bool bold = false)
        {
            return new BcCell { Text = text, Fore = fore, Bold = bold };
        }

        /// <summary>Ô tiền: giá trị 0 hiển thị chữ mờ, đánh dấu IsMoney để hiện tooltip đầy đủ khi cần.</summary>
        public static BcCell M(double v, bool bold = false)
        {
            return new BcCell
            {
                Text = v == 0 ? "0 đ" : BcTheme.Money(v),
                Fore = v == 0 ? BcTheme.Zero : (Color?)null,
                Bold = bold,
                IsMoney = true
            };
        }

        public static BcCell Rank(int rank)
        {
            Color back = BcTheme.TagBg, fore = BcTheme.Text2;
            if (rank == 1) { back = BcTheme.Brand; fore = Color.White; }
            else if (rank == 2) { back = BcTheme.Brand2; fore = Color.White; }
            else if (rank == 3) { back = BcTheme.Rank3; fore = Color.White; }
            return new BcCell { Kind = BcCellKind.Rank, Text = rank.ToString(BcTheme.Num), Back = back, Fore = fore };
        }

        public static BcCell Tag(string text, Color back, Color fore)
        {
            return new BcCell { Kind = BcCellKind.Tag, Text = text, Back = back, Fore = fore };
        }

        public static BcCell Share(double pct, double ratio)
        {
            return new BcCell { Kind = BcCellKind.Share, Text = BcTheme.Pct(pct), Ratio = Math.Max(0, Math.Min(1, ratio)) };
        }
    }

    public class BcRow
    {
        public BcCell[] Cells;
        public bool Total;
        public object Tag;
    }

    public class BcTable : BcControl
    {
        private List<BcColumn> _cols = new List<BcColumn>();
        private List<BcRow> _rows = new List<BcRow>();
        private int[] _widths = new int[0];
        private int _hoverRow = -2; // -2: không hover, -1: header, >=0: dòng dữ liệu
        private int _hoverCol = -1;
        private Point _mouse;

        public int RowHeight = 38;
        public int HeaderHeight = 36;
        public string EmptyText = "Không có dữ liệu phù hợp";

        public event EventHandler<BcRow> RowClick;

        private static Font CellFont { get { return BcTheme.F(8.75F); } }
        private static Font CellBold { get { return BcTheme.F(8.75F, FontStyle.Bold); } }
        private static Font HeadFont { get { return BcTheme.F(8.25F, FontStyle.Bold); } }
        private static Font TagFont { get { return BcTheme.F(8F, FontStyle.Bold); } }
        private const int CellPad = 6;
        private const int BarW = 42;

        public BcTable() { Height = HeaderHeight + 60 + 1; }

        public void SetData(List<BcColumn> cols, List<BcRow> rows)
        {
            _cols = cols ?? new List<BcColumn>();
            _rows = rows ?? new List<BcRow>();
            _hoverRow = -2;
            _hoverCol = -1;
            Height = HeaderHeight + (_rows.Count == 0 ? 60 : _rows.Count * RowHeight) + 1;
            ComputeWidths();
            Invalidate();
        }

        private int ContentNeedWidth(int c)
        {
            int w = BcTheme.Measure(_cols[c].Header, HeadFont) + 4;
            foreach (var r in _rows)
            {
                if (r.Cells == null || c >= r.Cells.Length || r.Cells[c] == null) continue;
                var cell = r.Cells[c];
                int cw;
                switch (cell.Kind)
                {
                    case BcCellKind.Rank: cw = 24; break;
                    case BcCellKind.Tag: cw = BcTheme.Measure(cell.Text, TagFont) + 18; break;
                    case BcCellKind.Share: cw = BcTheme.Measure("100.0%", CellFont) + 6 + BarW + 4; break;
                    default: cw = BcTheme.Measure(cell.Text, (cell.Bold || r.Total) ? CellBold : CellFont) + 4; break;
                }
                if (cw > w) w = cw;
            }
            return w + CellPad * 2;
        }

        private void ComputeWidths()
        {
            int n = _cols.Count;
            _widths = new int[n];
            if (n == 0) return;
            int avail = Math.Max(n * 20, Width - 2);
            var nat = new int[n];
            int sum = 0;
            for (int i = 0; i < n; i++) { nat[i] = ContentNeedWidth(i); sum += nat[i]; }

            if (sum <= avail)
            {
                float totalW = _cols.Sum(c => Math.Max(0.01f, c.Weight));
                int extra = avail - sum, used = 0;
                for (int i = 0; i < n; i++)
                {
                    int add = (int)(extra * Math.Max(0.01f, _cols[i].Weight) / totalW);
                    _widths[i] = nat[i] + add;
                    used += _widths[i];
                }
                _widths[n - 1] += avail - used;
            }
            else
            {
                // Khi khung hẹp hơn tổng chiều rộng tự nhiên:
                // Ưu tiên thu hẹp các cột chữ dài (Tên món, Danh mục, Họ tên) trước để cột số tiền & tiêu đề ngắn không bị "..."
                Array.Copy(nat, _widths, n);
                int deficit = sum - avail;
                int shrinkableRoom = 0;
                for (int i = 0; i < n; i++)
                {
                    if (_cols[i].Shrinkable && _widths[i] > 84)
                        shrinkableRoom += _widths[i] - 84;
                }

                if (shrinkableRoom > 0)
                {
                    int takeTotal = Math.Min(deficit, shrinkableRoom);
                    int taken = 0;
                    for (int i = 0; i < n; i++)
                    {
                        if (_cols[i].Shrinkable && _widths[i] > 84)
                        {
                            int room = _widths[i] - 84;
                            int cut = (int)((long)takeTotal * room / shrinkableRoom);
                            _widths[i] -= cut;
                            taken += cut;
                        }
                    }
                    deficit -= taken;
                }

                if (deficit > 0)
                {
                    int curSum = _widths.Sum();
                    int used = 0;
                    for (int i = 0; i < n; i++)
                    {
                        _widths[i] = Math.Max(24, (int)((long)_widths[i] * avail / Math.Max(1, curSum)));
                        used += _widths[i];
                    }
                    _widths[n - 1] = Math.Max(24, _widths[n - 1] + (avail - used));
                }
                else
                {
                    int used = _widths.Sum();
                    _widths[n - 1] += avail - used;
                }
            }
        }

        protected override void OnResize(EventArgs e) { base.OnResize(e); ComputeWidths(); }

        private bool IsTruncated(int rowIdx, int colIdx, out string label, out string fullText)
        {
            label = "";
            fullText = "";
            if (colIdx < 0 || colIdx >= _cols.Count || colIdx >= _widths.Length) return false;
            int innerW = Math.Max(4, _widths[colIdx] - CellPad * 2);

            if (rowIdx == -1)
            {
                string h = _cols[colIdx].Header ?? "";
                if (BcTheme.Measure(h, HeadFont) > innerW)
                {
                    label = "Cột";
                    fullText = h;
                    return true;
                }
                return false;
            }

            if (rowIdx < 0 || rowIdx >= _rows.Count) return false;
            var row = _rows[rowIdx];
            if (row.Cells == null || colIdx >= row.Cells.Length || row.Cells[colIdx] == null) return false;
            var cell = row.Cells[colIdx];
            fullText = cell.Text ?? "";
            if (string.IsNullOrEmpty(fullText)) return false;

            // Tìm tên định danh của dòng (cột Tên món / Họ tên / Kỳ)
            string rowName = "";
            for (int k = 0; k < row.Cells.Length; k++)
            {
                if (row.Cells[k] != null && row.Cells[k].Kind == BcCellKind.Text && row.Cells[k].Bold && !row.Cells[k].IsMoney)
                {
                    rowName = row.Cells[k].Text;
                    break;
                }
            }
            label = string.IsNullOrEmpty(rowName) || rowName == fullText
                ? _cols[colIdx].Header
                : (rowName + " · " + _cols[colIdx].Header);

            int textW;
            switch (cell.Kind)
            {
                case BcCellKind.Rank:
                    return false;
                case BcCellKind.Tag:
                    textW = BcTheme.Measure(fullText, TagFont) + 16;
                    return textW > innerW;
                case BcCellKind.Share:
                    textW = BcTheme.Measure(fullText, CellFont) + 6 + BarW;
                    return textW > innerW;
                default:
                    if (cell.IsMoney) return false;
                    textW = BcTheme.Measure(fullText, (cell.Bold || row.Total) ? CellBold : CellFont);
                    return textW > innerW;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            var outer = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            using (var path = BcTheme.Round(outer, 12))
            {
                using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
                g.SetClip(path);

                using (var br = new SolidBrush(BcTheme.HeaderBg)) g.FillRectangle(br, 0, 0, Width, HeaderHeight);
                using (var pen = new Pen(BcTheme.Line)) g.DrawLine(pen, 0, HeaderHeight - 1, Width, HeaderHeight - 1);

                int x = 1;
                for (int c = 0; c < _cols.Count; c++)
                {
                    var r = new Rectangle(x + CellPad, 0, Math.Max(4, _widths[c] - CellPad * 2), HeaderHeight);
                    BcTheme.DrawText(g, _cols[c].Header, HeadFont, r, BcTheme.Muted,
                        (_cols[c].Right ? TextFormatFlags.Right : TextFormatFlags.Left) | TextFormatFlags.EndEllipsis);
                    x += _widths[c];
                }

                if (_rows.Count == 0)
                {
                    BcTheme.DrawText(g, EmptyText, BcTheme.F(9F), new Rectangle(0, HeaderHeight, Width, Height - HeaderHeight), BcTheme.Muted, TextFormatFlags.HorizontalCenter);
                }

                for (int i = 0; i < _rows.Count; i++)
                {
                    var row = _rows[i];
                    int y = HeaderHeight + i * RowHeight;
                    if (row.Total)
                    {
                        using (var br = new SolidBrush(BcTheme.TotalBg)) g.FillRectangle(br, 0, y, Width, RowHeight);
                        using (var pen = new Pen(BcTheme.Line)) g.DrawLine(pen, 0, y, Width, y);
                    }
                    else if (i == _hoverRow)
                    {
                        using (var br = new SolidBrush(BcTheme.HoverBg)) g.FillRectangle(br, 0, y, Width, RowHeight);
                    }
                    if (i < _rows.Count - 1 && !_rows[i + 1].Total)
                        using (var pen = new Pen(BcTheme.Line2)) g.DrawLine(pen, 0, y + RowHeight - 1, Width, y + RowHeight - 1);

                    x = 1;
                    for (int c = 0; c < _cols.Count; c++)
                    {
                        if (row.Cells != null && c < row.Cells.Length && row.Cells[c] != null)
                            PaintCell(g, row.Cells[c], new Rectangle(x + CellPad, y, Math.Max(4, _widths[c] - CellPad * 2), RowHeight), _cols[c].Right, row.Total);
                        x += _widths[c];
                    }
                }
                g.ResetClip();
                using (var pen = new Pen(BcTheme.Line)) g.DrawPath(pen, path);
            }

            // Vẽ hộp tooltip nổi khi rê chuột vào ô bị "..." hoặc ô số tiền
            string tipLabel, tipValue;
            if (_hoverCol >= 0 && IsTruncated(_hoverRow, _hoverCol, out tipLabel, out tipValue))
                DrawCellTooltip(g, tipLabel, tipValue);
        }

        private void DrawCellTooltip(Graphics g, string label, string value)
        {
            var f1 = BcTheme.F(8F);
            var f2 = BcTheme.F(9.5F, FontStyle.Bold);
            int w = Math.Max(BcTheme.Measure(label, f1), BcTheme.Measure(value, f2)) + 26;
            w = Math.Max(110, Math.Min(Width - 12, w));
            int h = string.IsNullOrEmpty(label) ? 30 : 46;

            int x = _mouse.X - w / 2;
            x = Math.Max(6, Math.Min(x, Width - w - 6));
            int y = _mouse.Y - h - 10;
            if (y < 4) y = Math.Min(Height - h - 4, _mouse.Y + 18);

            var box = new RectangleF(x, y, w, h);
            BcTheme.SoftShadow(g, box, 9);
            BcTheme.FillRound(g, box, 9, Color.White, BcTheme.BrandBorder);

            if (string.IsNullOrEmpty(label))
            {
                BcTheme.DrawText(g, value, f2, new Rectangle(x + 12, y, w - 24, h), BcTheme.Text, TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
            }
            else
            {
                BcTheme.DrawText(g, label, f1, new Rectangle(x + 12, y + 5, w - 24, 16), BcTheme.Muted, TextFormatFlags.EndEllipsis);
                BcTheme.DrawText(g, value, f2, new Rectangle(x + 12, y + 21, w - 24, 20), BcTheme.Brand, TextFormatFlags.EndEllipsis);
            }
        }

        private void PaintCell(Graphics g, BcCell cell, Rectangle r, bool right, bool total)
        {
            switch (cell.Kind)
            {
                case BcCellKind.Rank:
                {
                    var box = new RectangleF(r.X, r.Y + (r.Height - 22) / 2f, 22, 22);
                    BcTheme.FillRound(g, box, 6, cell.Back);
                    BcTheme.DrawText(g, cell.Text, BcTheme.F(8.5F, FontStyle.Bold), Rectangle.Round(box), cell.Fore ?? BcTheme.Text2, TextFormatFlags.HorizontalCenter);
                    break;
                }
                case BcCellKind.Tag:
                {
                    int tw = Math.Min(r.Width, BcTheme.Measure(cell.Text, TagFont) + 16);
                    var pill = new RectangleF(right ? r.Right - tw : r.X, r.Y + (r.Height - 21) / 2f, tw, 21);
                    BcTheme.FillRound(g, pill, 10.5f, cell.Back);
                    BcTheme.DrawText(g, cell.Text, TagFont, new Rectangle((int)pill.X + 6, (int)pill.Y, Math.Max(4, (int)pill.Width - 12), (int)pill.Height),
                        cell.Fore ?? BcTheme.Text2, TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
                    break;
                }
                case BcCellKind.Share:
                {
                    var track = new RectangleF(r.Right - BarW, r.Y + r.Height / 2f - 3, BarW, 6);
                    BcTheme.FillRound(g, track, 3, BcTheme.TrackBg);
                    if (cell.Ratio > 0)
                        BcTheme.FillRound(g, new RectangleF(track.X, track.Y, Math.Max(5, (float)(BarW * cell.Ratio)), 6), 3, BcTheme.Brand2);
                    BcTheme.DrawText(g, cell.Text, CellFont, new Rectangle(r.X, r.Y, Math.Max(4, r.Width - BarW - 6), r.Height), cell.Fore ?? BcTheme.Text, TextFormatFlags.Right);
                    break;
                }
                default:
                    BcTheme.DrawText(g, cell.Text, (cell.Bold || total) ? CellBold : CellFont, r, cell.Fore ?? BcTheme.Text,
                        (right ? TextFormatFlags.Right : TextFormatFlags.Left) | TextFormatFlags.EndEllipsis);
                    break;
            }
        }

        private void HitCell(Point p, out int rowIdx, out int colIdx)
        {
            colIdx = -1;
            int x = 1;
            for (int c = 0; c < _widths.Length; c++)
            {
                if (p.X >= x && p.X < x + _widths[c]) { colIdx = c; break; }
                x += _widths[c];
            }
            if (p.Y < HeaderHeight)
            {
                rowIdx = -1;
                return;
            }
            int i = (p.Y - HeaderHeight) / RowHeight;
            rowIdx = (i >= 0 && i < _rows.Count) ? i : -2;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _mouse = e.Location;
            int r, c;
            HitCell(e.Location, out r, out c);
            if (r != _hoverRow || c != _hoverCol)
            {
                _hoverRow = r;
                _hoverCol = c;
            }
            Invalidate();
            Cursor = r >= 0 && r < _rows.Count && !_rows[r].Total && RowClick != null ? Cursors.Hand : Cursors.Default;
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hoverRow = -2;
            _hoverCol = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int r, c;
            HitCell(e.Location, out r, out c);
            if (r >= 0 && r < _rows.Count && !_rows[r].Total)
                RowClick?.Invoke(this, _rows[r]);
            base.OnMouseClick(e);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  PHÂN TRANG: "Hiển thị 1–10 trên 11 nhân viên"   ‹ 1 2 ›
    // ───────────────────────────────────────────────────────────────────────
    public class BcPager : BcControl
    {
        private readonly List<KeyValuePair<Rectangle, int>> _btns = new List<KeyValuePair<Rectangle, int>>();
        private int _hoverTarget = -1;

        public int Total { get; private set; }
        public int Page { get; private set; } = 1;
        public int PageSize { get; set; } = 10;
        public string Unit { get; set; } = "dòng";

        public event EventHandler PageChanged;

        public BcPager() { Height = 46; }

        public int PageCount { get { return Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize)); } }

        public void SetState(int total, int page)
        {
            Total = Math.Max(0, total);
            Page = Math.Max(1, Math.Min(page, PageCount));
            Invalidate();
        }

        private List<int> Tokens()
        {
            int pc = PageCount;
            var list = new List<int>();
            if (pc <= 7) { for (int i = 1; i <= pc; i++) list.Add(i); return list; }
            list.Add(1);
            int a = Math.Max(2, Page - 1), b = Math.Min(pc - 1, Page + 1);
            if (a > 2) list.Add(0);
            for (int i = a; i <= b; i++) list.Add(i);
            if (b < pc - 1) list.Add(0);
            list.Add(pc);
            return list;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            int from = Total == 0 ? 0 : (Page - 1) * PageSize + 1;
            int to = Math.Min(Total, Page * PageSize);
            string info = Total == 0 ? "Không có " + Unit + " phù hợp" : "Hiển thị " + from + BcTheme.EnDash + to + " trên " + Total + " " + Unit;
            BcTheme.DrawText(g, info, BcTheme.F(9F), new Rectangle(2, 12, Width / 2, 30), BcTheme.Muted);

            var tokens = new List<int> { -1 };
            tokens.AddRange(Tokens());
            tokens.Add(-2);
            const int bw = 30, gap = 6;
            int x = Width - 1 - (tokens.Count * bw + (tokens.Count - 1) * gap);
            _btns.Clear();
            var f = BcTheme.F(9F, FontStyle.Bold);
            foreach (var t in tokens)
            {
                var r = new Rectangle(x, 12, bw, 30);
                int target = t == -1 ? Page - 1 : t == -2 ? Page + 1 : t;
                bool valid = t != 0 && target >= 1 && target <= PageCount && target != Page;
                bool on = t > 0 && t == Page;
                string label = t == -1 ? "\u2039" : t == -2 ? "\u203A" : t == 0 ? "\u2026" : t.ToString(BcTheme.Num);
                if (t != 0)
                {
                    Color fill = on ? BcTheme.Brand : (valid && _hoverTarget == target ? BcTheme.SegBg : Color.White);
                    BcTheme.FillRound(g, new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), 8, fill, on ? BcTheme.Brand : BcTheme.Line);
                }
                Color fc = on ? Color.White : (t == 0 || valid ? BcTheme.Text2 : BcTheme.Zero);
                BcTheme.DrawText(g, label, t < 0 ? BcTheme.F(10.5F, FontStyle.Bold) : f, r, fc, TextFormatFlags.HorizontalCenter);
                if (valid) _btns.Add(new KeyValuePair<Rectangle, int>(r, target));
                x += bw + gap;
            }
        }

        private int Hit(Point p) { foreach (var b in _btns) if (b.Key.Contains(p)) return b.Value; return -1; }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = Hit(e.Location);
            Cursor = h > 0 ? Cursors.Hand : Cursors.Default;
            if (h != _hoverTarget) { _hoverTarget = h; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { _hoverTarget = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int h = Hit(e.Location);
            if (h > 0 && h != Page)
            {
                Page = h;
                Invalidate();
                PageChanged?.Invoke(this, EventArgs.Empty);
            }
            base.OnMouseClick(e);
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  BIỂU ĐỒ ĐƯỜNG CONG (MONOTONE CUBIC): Thu – Chi – Lợi nhuận theo quý
    // ───────────────────────────────────────────────────────────────────────
    public class BcQuarterChart : BcControl
    {
        private double[] _thu = new double[4], _chi = new double[4], _ln = new double[4];
        private int _year;
        private int _hover = -1;
        private const int L = 52, R = 16, T = 12, B = 70;

        public void SetData(double[] thu, double[] chi, double[] ln, int year)
        {
            _thu = thu ?? new double[4];
            _chi = chi ?? new double[4];
            _ln = ln ?? new double[4];
            _year = year;
            Invalidate();
        }

        private Rectangle Plot { get { return new Rectangle(L, T, Math.Max(1, Width - L - R), Math.Max(1, Height - T - B)); } }

        private static GraphicsPath MonoPath(PointF[] p)
        {
            var path = new GraphicsPath();
            int n = p.Length;
            if (n < 2) return path;
            var dx = new float[n - 1]; var m = new float[n - 1]; var t = new float[n];
            for (int i = 0; i < n - 1; i++)
            {
                dx[i] = p[i + 1].X - p[i].X;
                m[i] = dx[i] == 0 ? 0 : (p[i + 1].Y - p[i].Y) / dx[i];
            }
            t[0] = m[0];
            t[n - 1] = m[n - 2];
            for (int i = 1; i < n - 1; i++) t[i] = m[i - 1] * m[i] <= 0 ? 0 : (m[i - 1] + m[i]) / 2;
            for (int i = 0; i < n - 1; i++)
            {
                if (m[i] == 0) { t[i] = 0; t[i + 1] = 0; continue; }
                float a = t[i] / m[i], b = t[i + 1] / m[i], s = a * a + b * b;
                if (s > 9) { float k = 3 / (float)Math.Sqrt(s); t[i] = k * a * m[i]; t[i + 1] = k * b * m[i]; }
            }
            for (int i = 0; i < n - 1; i++)
            {
                float h = dx[i] / 3;
                path.AddBezier(p[i], new PointF(p[i].X + h, p[i].Y + t[i] * h), new PointF(p[i + 1].X - h, p[i + 1].Y - t[i + 1] * h), p[i + 1]);
            }
            return path;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            var plot = Plot;
            if (plot.Width < 60 || plot.Height < 40) return;
            int n = 4;
            double max = Math.Max(_thu.Max(), _chi.Max());
            double step = BcTheme.NiceStep(max > 0 ? max : 5e6, 5);
            double top = Math.Ceiling(max / step) * step;
            if (top <= 0) top = step * 5;
            float baseY = plot.Bottom;
            Func<int, float> X = i => plot.X + plot.Width * (i + 0.5f) / n;
            Func<double, float> Y = v => plot.Y + (float)(plot.Height * (1 - v / top));

            var fAxis = BcTheme.F(8.25F);
            using (var pen = new Pen(BcTheme.Grid))
            {
                for (double v = 0; v <= top + step / 2; v += step)
                {
                    float y = Y(v);
                    g.DrawLine(pen, plot.X, y, plot.Right, y);
                    BcTheme.DrawText(g, BcTheme.MoneyAxis(v), fAxis, new Rectangle(0, (int)y - 9, L - 8, 18), BcTheme.Axis, TextFormatFlags.Right);
                }
            }

            if (_hover >= 0)
                using (var pen = new Pen(Color.FromArgb(0xE7, 0xB9, 0xA0), 1f) { DashPattern = new float[] { 3, 4 } })
                    g.DrawLine(pen, X(_hover), plot.Y, X(_hover), baseY);

            var thu = Enumerable.Range(0, n).Select(i => new PointF(X(i), Y(_thu[i]))).ToArray();
            var chi = Enumerable.Range(0, n).Select(i => new PointF(X(i), Y(_chi[i]))).ToArray();

            FillArea(g, chi, baseY, plot.Y, BcTheme.Amber, 36);
            FillArea(g, thu, baseY, plot.Y, BcTheme.Brand, 46);

            using (var path = MonoPath(chi))
            using (var pen = new Pen(BcTheme.Amber, 2.5f) { DashPattern = new float[] { 2.4f, 2f }, DashCap = DashCap.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawPath(pen, path);
            using (var path = MonoPath(thu))
            using (var pen = new Pen(BcTheme.Brand, 3f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                g.DrawPath(pen, path);

            float slot = plot.Width / (float)n;
            for (int i = 0; i < n; i++)
            {
                bool hv = i == _hover;
                DrawDot(g, chi[i], hv ? 5f : 3.5f, BcTheme.Amber, 2f);
                DrawDot(g, thu[i], hv ? 6f : 4f, BcTheme.Brand, hv ? 3f : 2f);

                int cx = (int)X(i);
                BcTheme.DrawText(g, BcTheme.QuarterNames[i], hv ? BcTheme.F(9F, FontStyle.Bold) : BcTheme.F(9F),
                    new Rectangle(cx - 46, (int)baseY + 8, 92, 20), hv ? BcTheme.Brand : Color.FromArgb(0x8A, 0x77, 0x68), TextFormatFlags.HorizontalCenter);

                double ln = _ln[i];
                bool pos = ln >= 0;
                string lbl = "LN " + (ln == 0 ? "0" : (pos ? "+" : BcTheme.Minus) + BcTheme.Tr(Math.Abs(ln)));
                float pw = Math.Min(86, slot - 6);
                var pill = new RectangleF(cx - pw / 2, baseY + 32, pw, 22);
                BcTheme.FillRound(g, pill, 11, pos ? BcTheme.GreenSoft : BcTheme.RedSoft);
                BcTheme.DrawText(g, lbl, BcTheme.F(8.25F, FontStyle.Bold), Rectangle.Round(pill), pos ? BcTheme.Green : BcTheme.Red, TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis);
            }

            if (_hover >= 0) DrawTooltip(g, _hover, X(_hover), plot);
        }

        private static void FillArea(Graphics g, PointF[] pts, float baseY, float topY, Color c, int alpha)
        {
            using (var path = MonoPath(pts))
            {
                path.AddLine(pts[pts.Length - 1].X, pts[pts.Length - 1].Y, pts[pts.Length - 1].X, baseY);
                path.AddLine(pts[pts.Length - 1].X, baseY, pts[0].X, baseY);
                path.CloseFigure();
                var rect = new RectangleF(0, topY - 1, 10, Math.Max(2, baseY - topY + 2));
                using (var br = new LinearGradientBrush(rect, Color.FromArgb(alpha, c), Color.FromArgb(0, c), LinearGradientMode.Vertical))
                {
                    br.WrapMode = WrapMode.TileFlipXY;
                    g.FillPath(br, path);
                }
            }
        }

        private static void DrawDot(Graphics g, PointF p, float r, Color c, float w)
        {
            var rc = new RectangleF(p.X - r, p.Y - r, r * 2, r * 2);
            using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, rc);
            using (var pen = new Pen(c, w)) g.DrawEllipse(pen, rc);
        }

        private void DrawTooltip(Graphics g, int i, float px, Rectangle plot)
        {
            const int w = 236, h = 90;
            float bx = px - w - 14;
            if (bx < 4) bx = px + 14;
            if (bx + w > Width - 4) bx = Width - 4 - w;
            var box = new RectangleF(bx, plot.Y + 4, w, h);
            BcTheme.SoftShadow(g, box, 12);
            BcTheme.FillRound(g, box, 12, Color.White, Color.FromArgb(0xF1, 0xE6, 0xDD));
            int x = (int)box.X, y = (int)box.Y;
            BcTheme.DrawText(g, BcTheme.QuarterNames[i] + " / " + _year, BcTheme.F(8.5F), new Rectangle(x + 12, y + 7, w - 24, 18), BcTheme.Muted);

            var rows = new[]
            {
                new { L = "Tổng thu", V = BcTheme.Money(_thu[i]), C = BcTheme.Brand, VC = BcTheme.Text },
                new { L = "Tổng chi", V = BcTheme.Money(_chi[i]), C = BcTheme.Amber, VC = BcTheme.Text },
                new { L = "Lợi nhuận", V = BcTheme.SignedMoney(_ln[i]), C = BcTheme.Green, VC = _ln[i] >= 0 ? BcTheme.Green : BcTheme.Red }
            };
            for (int k = 0; k < rows.Length; k++)
            {
                int ry = y + 28 + k * 19;
                using (var br = new SolidBrush(rows[k].C)) g.FillEllipse(br, x + 12, ry + 5, 8, 8);
                BcTheme.DrawText(g, rows[k].L, BcTheme.F(8.75F), new Rectangle(x + 26, ry, 86, 18), BcTheme.Text2);
                BcTheme.DrawText(g, rows[k].V, BcTheme.F(8.75F, FontStyle.Bold), new Rectangle(x + 92, ry, w - 104, 18), rows[k].VC, TextFormatFlags.Right);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var plot = Plot;
            int h = -1;
            if (e.X >= plot.X && e.X <= plot.Right && e.Y >= plot.Y - 4 && e.Y <= Height)
                h = Math.Max(0, Math.Min(3, (int)((e.X - plot.X) / (plot.Width / 4f))));
            if (h != _hover) { _hover = h; Invalidate(); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  BIỂU ĐỒ THANH NGANG XẾP CHỒNG (TOP 10) – dùng chung cho nhân viên & món ăn
    // ───────────────────────────────────────────────────────────────────────
    public class BcBarItem
    {
        public string Name;
        public string Sub;
        public double[] Values = new double[0];
        public double Total;
        public double Share;
        public object Tag;
    }

    public class BcStackedBarChart : BcControl
    {
        private List<BcBarItem> _items = new List<BcBarItem>();
        private int _hover = -1;
        private Point _mouse;
        private const int RowH = 42, T = 8, L = 224, R = 180, AxisH = 32;

        public Color[] Colors = BcTheme.QuarterColors;
        public string[] SeriesNames = BcTheme.QuarterNames;
        public Func<double, string> FormatValue = BcTheme.Money;
        public Func<double, string> FormatAxis = BcTheme.MoneyAxis;
        public string EmptyText = "Chưa phát sinh doanh thu";
        public string NoDataText = "Không có dữ liệu phù hợp";

        public event EventHandler<BcBarItem> ItemClick;

        public BcStackedBarChart() { Height = 120; }

        public void SetItems(List<BcBarItem> items)
        {
            _items = items ?? new List<BcBarItem>();
            _hover = -1;
            Height = _items.Count == 0 ? 120 : T + _items.Count * RowH + AxisH;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = Prepare(e);
            if (_items.Count == 0)
            {
                BcTheme.DrawText(g, NoDataText, BcTheme.F(9F), ClientRectangle, BcTheme.Muted, TextFormatFlags.HorizontalCenter);
                return;
            }
            int iw = Math.Max(40, Width - L - R);
            double max = _items.Max(i => i.Total);
            double step = BcTheme.NiceStep(max, 5);
            double top = max > 0 ? Math.Ceiling(max / step) * step : 0;
            Func<double, float> X = v => top > 0 ? L + (float)(iw * v / top) : L;
            int gridBottom = T + _items.Count * RowH;

            if (top > 0)
            {
                using (var pen = new Pen(BcTheme.Grid))
                {
                    for (double v = 0; v <= top + step / 2; v += step)
                    {
                        float x = X(v);
                        g.DrawLine(pen, x, T, x, gridBottom + 4);
                        BcTheme.DrawText(g, FormatAxis(v), BcTheme.F(8.25F), new Rectangle((int)x - 42, gridBottom + 8, 84, 18), BcTheme.Axis, TextFormatFlags.HorizontalCenter);
                    }
                }
            }

            for (int i = 0; i < _items.Count; i++)
            {
                var it = _items[i];
                int cy = T + i * RowH + RowH / 2;
                bool has = it.Total > 0;
                if (i == _hover && has)
                    BcTheme.FillRound(g, new RectangleF(0, cy - RowH / 2 + 2, Width - 1, RowH - 4), 8, Color.FromArgb(0xFF, 0xF8, 0xF3));

                BcTheme.DrawText(g, (i + 1).ToString("00", BcTheme.Num), BcTheme.F(9.25F, FontStyle.Bold), new Rectangle(6, cy - 10, 28, 20), i < 3 ? BcTheme.Brand : BcTheme.Axis);
                BcTheme.DrawText(g, it.Name, BcTheme.F(9.25F, FontStyle.Bold), new Rectangle(38, cy - 17, L - 46, 17), has ? BcTheme.Text : BcTheme.Faded, TextFormatFlags.EndEllipsis);
                BcTheme.DrawText(g, it.Sub, BcTheme.F(7.75F), new Rectangle(38, cy + 2, L - 46, 15), BcTheme.Axis, TextFormatFlags.EndEllipsis);

                if (!has)
                {
                    BcTheme.FillRound(g, new RectangleF(L, cy - 9, iw, 18), 9, BcTheme.EmptyBar);
                    BcTheme.DrawText(g, EmptyText, BcTheme.F(8.25F), new Rectangle(L + 12, cy - 9, iw - 24, 18), BcTheme.Faded);
                    continue;
                }

                float barW = Math.Max(4, X(it.Total) - L);
                var bar = new RectangleF(L, cy - 10, barW, 20);
                using (var clip = BcTheme.Round(bar, 6.5f))
                {
                    var state = g.Save();
                    g.SetClip(clip);
                    double acc = 0;
                    for (int k = 0; k < it.Values.Length; k++)
                    {
                        double v = it.Values[k];
                        if (v <= 0) continue;
                        float x0 = X(acc), x1 = X(acc + v);
                        using (var br = new SolidBrush(k < Colors.Length ? Colors[k] : BcTheme.Brand))
                            g.FillRectangle(br, x0, bar.Y, Math.Max(1, x1 - x0 + 0.6f), bar.Height);
                        acc += v;
                    }
                    g.Restore(state);
                }
                BcTheme.DrawText(g, FormatValue(it.Total), BcTheme.F(9.25F, FontStyle.Bold), new Rectangle((int)bar.Right + 10, cy - 10, R - 68, 20), BcTheme.Text);
                BcTheme.DrawText(g, BcTheme.Pct(it.Share), BcTheme.F(9F, FontStyle.Bold), new Rectangle(Width - 62, cy - 10, 56, 20), BcTheme.Muted, TextFormatFlags.Right);
            }

            if (_hover >= 0 && _hover < _items.Count && _items[_hover].Total > 0) DrawTooltip(g, _items[_hover], T + _hover * RowH + RowH / 2);
        }

        private void DrawTooltip(Graphics g, BcBarItem it, int cy)
        {
            int n = Math.Min(SeriesNames.Length, it.Values.Length);
            int w = 236, h = 34 + n * 18 + 8;
            int x = _mouse.X + 16;
            if (x + w > Width - 4) x = _mouse.X - 16 - w;
            x = Math.Max(4, x);
            int y = Math.Max(2, Math.Min(cy + 14, Height - h - 2));
            if (y < cy + 14 && cy - 14 - h >= 2) y = cy - 14 - h;
            var box = new RectangleF(x, y, w, h);
            BcTheme.SoftShadow(g, box, 12);
            BcTheme.FillRound(g, box, 12, Color.White, Color.FromArgb(0xF1, 0xE6, 0xDD));
            BcTheme.DrawText(g, it.Name, BcTheme.F(8.75F, FontStyle.Bold), new Rectangle(x + 12, y + 7, w - 24, 18), BcTheme.Text2, TextFormatFlags.EndEllipsis);
            for (int k = 0; k < n; k++)
            {
                int ry = y + 29 + k * 18;
                BcTheme.FillRound(g, new RectangleF(x + 12, ry + 5, 8, 8), 2, k < Colors.Length ? Colors[k] : BcTheme.Brand);
                BcTheme.DrawText(g, SeriesNames[k], BcTheme.F(8.5F), new Rectangle(x + 26, ry, 92, 18), BcTheme.Text2);
                BcTheme.DrawText(g, FormatValue(it.Values[k]), BcTheme.F(8.5F, FontStyle.Bold), new Rectangle(x + 92, ry, w - 104, 18),
                    it.Values[k] == 0 ? BcTheme.Zero : BcTheme.Text, TextFormatFlags.Right);
            }
        }

        private int Hit(Point p)
        {
            if (p.Y < T) return -1;
            int i = (p.Y - T) / RowH;
            return i >= 0 && i < _items.Count ? i : -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            _mouse = e.Location;
            int h = Hit(e.Location);
            bool clickable = h >= 0 && _items[h].Total > 0 && ItemClick != null;
            Cursor = clickable ? Cursors.Hand : Cursors.Default;
            _hover = h;
            Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int h = Hit(e.Location);
            if (h >= 0 && _items[h].Total > 0) ItemClick?.Invoke(this, _items[h]);
            base.OnMouseClick(e);
        }
    }
}
