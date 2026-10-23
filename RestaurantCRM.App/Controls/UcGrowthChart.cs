using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Biểu đồ đường cong mượt (line + area mờ) trong thẻ trắng bo góc.
    /// Di chuột để xem giá trị từng điểm; mặc định làm nổi bật điểm cao nhất.
    /// </summary>
    public class UcLineChart : RoundedPanel
    {
        private string[] _labels = new string[0];
        private double[] _values = new double[0];
        private int _hover = -1;
        private string _title = "Doanh thu theo tháng";
        private string _caption = "";
        private string _unit = "đ";
        private bool _loading = true;

        public UcLineChart()
        {
            Radius = 14;
            Shadow = 8;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(120, ThemeManager.Line);
            Size = new Size(600, 300);
            DoubleBuffered = true;
        }

        public string Title { get { return _title; } set { _title = value; Invalidate(); } }
        public string Caption { get { return _caption; } set { _caption = value; Invalidate(); } }
        public string Unit { get { return _unit; } set { _unit = value; Invalidate(); } }

        public void SetData(string[] labels, double[] values)
        {
            _labels = labels ?? new string[0];
            _values = values ?? new double[0];
            _loading = false;
            _hover = -1;
            Invalidate();
        }

        private Rectangle PlotRect
        {
            get
            {
                var c = CardRect;
                return new Rectangle(c.X + 64, c.Y + 70, Math.Max(10, c.Width - 64 - 28), Math.Max(10, c.Height - 70 - 44));
            }
        }

        private double MaxValue()
        {
            double m = 0;
            foreach (var v in _values) if (v > m) m = v;
            if (m <= 0) return 1;
            // làm tròn lên cho trục Y đẹp
            double mag = Math.Pow(10, Math.Floor(Math.Log10(m)));
            double step = mag / 2;
            return Math.Ceiling(m * 1.1 / step) * step;
        }

        private PointF[] Points()
        {
            var p = PlotRect;
            int n = _values.Length;
            var pts = new PointF[n];
            double max = MaxValue();
            for (int i = 0; i < n; i++)
            {
                float x = n == 1 ? p.X + p.Width / 2f : p.X + p.Width * i / (float)(n - 1);
                float y = p.Bottom - (float)(_values[i] / max * p.Height);
                pts[i] = new PointF(x, y);
            }
            return pts;
        }

        private static string Compact(double v)
        {
            if (v >= 1000000000) return (v / 1000000000).ToString("0.#") + " tỷ";
            if (v >= 1000000) return (v / 1000000).ToString("0.#") + " tr";
            if (v >= 1000) return (v / 1000).ToString("0.#") + " k";
            return v.ToString("0");
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_values.Length == 0) return;
            var pts = Points();
            int best = -1; float bestD = float.MaxValue;
            for (int i = 0; i < pts.Length; i++)
            {
                float d = Math.Abs(pts[i].X - e.X);
                if (d < bestD) { bestD = d; best = i; }
            }
            if (!PlotRect.Contains(e.Location) && bestD > 30) best = -1;
            if (best != _hover) { _hover = best; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != -1) { _hover = -1; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var card = CardRect;

            // Tiêu đề + chú thích bên phải
            using (var f = new Font("Segoe UI", 11.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, _title, f, new Point(card.X + 24, card.Y + 22), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            if (!string.IsNullOrEmpty(_caption))
            {
                using (var fi = ThemeManager.Icon(8.5F))
                using (var f = new Font("Segoe UI", 8.5F, FontStyle.Bold))
                {
                    var ts = TextRenderer.MeasureText(g, _caption, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    int cx = card.Right - 24 - ts.Width;
                    TextRenderer.DrawText(g, _caption, f, new Point(cx, card.Y + 25), ThemeManager.Primary, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, ThemeManager.Glyph.Calendar, fi, new Point(cx - 20, card.Y + 25), ThemeManager.Primary, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }

            var p = PlotRect;
            double max = MaxValue();
            bool allZero = true;
            foreach (var v in _values) if (v > 0) { allZero = false; break; }

            // Lưới ngang + nhãn trục Y
            using (var pen = new Pen(Color.FromArgb(240, 242, 247), 1))
            using (var f = new Font("Segoe UI", 7.5F))
            {
                for (int i = 0; i <= 4; i++)
                {
                    int y = p.Bottom - p.Height * i / 4;
                    g.DrawLine(pen, p.X, y, p.Right, y);
                    string lbl = _values.Length == 0 ? "" : (allZero ? (i == 0 ? "0" : "") : Compact(max * i / 4));
                    TextRenderer.DrawText(g, lbl, f, new Rectangle(card.X + 8, y - 8, 48, 16), ThemeManager.Muted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }

            if (_loading || _values.Length == 0)
            {
                using (var f = new Font("Segoe UI", 9F))
                    TextRenderer.DrawText(g, _loading ? "Đang tải dữ liệu..." : "Chưa có dữ liệu", f, p, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }
            if (allZero)
            {
                using (var f = new Font("Segoe UI", 9F))
                    TextRenderer.DrawText(g, "Chưa có doanh thu trong năm nay", f, p, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            var pts = Points();
            int focus = allZero ? -1 : _hover;
            if (focus < 0 && !allZero)
            {
                double best = -1;
                for (int i = 0; i < _values.Length; i++) if (_values[i] > best) { best = _values[i]; focus = i; }
                if (best <= 0) focus = -1;
            }

            // Nhãn trục X
            using (var f = new Font("Segoe UI", 8F))
            using (var fb = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                for (int i = 0; i < pts.Length && i < _labels.Length; i++)
                {
                    bool on = i == focus;
                    TextRenderer.DrawText(g, _labels[i], on ? fb : f, new Rectangle((int)pts[i].X - 24, p.Bottom + 12, 48, 18),
                        on ? ThemeManager.Primary : ThemeManager.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }

            // Vùng tô mờ dưới đường
            if (pts.Length > 1)
            {
                using (var area = new GraphicsPath())
                {
                    area.AddCurve(pts, 0.45f);
                    area.AddLine(pts[pts.Length - 1].X, pts[pts.Length - 1].Y, pts[pts.Length - 1].X, p.Bottom);
                    area.AddLine(pts[pts.Length - 1].X, p.Bottom, pts[0].X, p.Bottom);
                    area.CloseFigure();
                    using (var br = new LinearGradientBrush(new Rectangle(p.X, p.Y - 1, p.Width, p.Height + 2),
                        Color.FromArgb(46, ThemeManager.Primary), Color.FromArgb(0, ThemeManager.Primary), LinearGradientMode.Vertical))
                        g.FillPath(br, area);
                }
                using (var pen = new Pen(ThemeManager.Primary, 2.4f) { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawCurve(pen, pts, 0.45f);
            }

            // Điểm nổi bật + tooltip
            if (focus >= 0 && focus < pts.Length)
            {
                var pt = pts[focus];
                using (var pen = new Pen(Color.FromArgb(70, ThemeManager.Primary), 1) { DashStyle = DashStyle.Dash })
                    g.DrawLine(pen, pt.X, pt.Y + 6, pt.X, p.Bottom);
                using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, pt.X - 6, pt.Y - 6, 12, 12);
                using (var pen = new Pen(ThemeManager.Primary, 3f)) g.DrawEllipse(pen, pt.X - 5, pt.Y - 5, 10, 10);

                string top = focus < _labels.Length ? _labels[focus] : "";
                string val = _values[focus].ToString("N0") + " " + _unit;
                using (var f1 = new Font("Segoe UI", 7.5F))
                using (var f2 = new Font("Segoe UI", 9F, FontStyle.Bold))
                {
                    var s1 = TextRenderer.MeasureText(g, top, f1, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    var s2 = TextRenderer.MeasureText(g, val, f2, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    int w = Math.Max(s1.Width, s2.Width) + 24, h = 46;
                    int tx = (int)pt.X - w / 2;
                    tx = Math.Max(card.X + 8, Math.Min(card.Right - w - 8, tx));
                    int ty = (int)pt.Y - h - 14;
                    if (ty < card.Y + 50) ty = (int)pt.Y + 14;
                    var tip = new Rectangle(tx, ty, w, h);
                    ThemeManager.DrawSoftShadow(g, tip, 8, 6, 22);
                    using (var path = ThemeManager.RoundedRect(tip, 8))
                    using (var br = new SolidBrush(Color.White))
                        g.FillPath(br, path);
                    TextRenderer.DrawText(g, top, f1, new Point(tip.X + 12, tip.Y + 7), ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, val, f2, new Point(tip.X + 12, tip.Y + 22), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }
        }
    }

    /// <summary>Tên cũ — giữ để tương thích.</summary>
    public class UcGrowthChart : UcLineChart { }
}