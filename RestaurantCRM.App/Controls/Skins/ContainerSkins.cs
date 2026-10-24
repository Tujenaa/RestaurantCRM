using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    // ───────────────────────────────────────────────────────────────────────
    //  BORDER SKIN (Panel / PictureBox viền mỏng bo góc)
    // ───────────────────────────────────────────────────────────────────────
    public static class BorderSkin
    {
        public static void Attach(Control c)
        {
            var p = c as Panel;
            var pic = c as PictureBox;
            if (p != null && p.BorderStyle == BorderStyle.FixedSingle) p.BorderStyle = BorderStyle.None;
            else if (pic != null && pic.BorderStyle == BorderStyle.FixedSingle) pic.BorderStyle = BorderStyle.None;
            else return;
            c.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new RectangleF(0.5f, 0.5f, c.Width - 1.5f, c.Height - 1.5f);
                using (var outer = new GraphicsPath())
                {
                    // tô góc ngoài bằng màu nền cha để viền trông bo tròn
                    outer.AddRectangle(new RectangleF(-1, -1, c.Width + 2, c.Height + 2));
                    using (var inner = SkinUtil.Round(r, 10)) outer.AddPath(inner, false);
                    using (var br = new SolidBrush(SkinUtil.Backdrop(c))) g.FillPath(br, outer);
                }
                using (var path = SkinUtil.Round(r, 10))
                using (var pen = new Pen(ThemeManager.Line)) g.DrawPath(pen, path);
            };
            c.Resize += (s, e) => c.Invalidate();
            c.Invalidate();
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  GROUPBOX -> TIÊU ĐỀ MỤC
    // ───────────────────────────────────────────────────────────────────────
    public static class GroupSkin
    {
        public static void Attach(GroupBox gb)
        {
            gb.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(gb.BackColor.A == 255 ? gb.BackColor : SkinUtil.Backdrop(gb));
                string title = SkinUtil.CleanText((gb.Text ?? "").Replace("&&", "&"));
                using (var f = new Font(ThemeManager.FontFamily, 9.5F, FontStyle.Bold))
                {
                    TextRenderer.DrawText(g, title, f, new Point(0, 0), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    var ts = TextRenderer.MeasureText(g, title, f, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    int y = ts.Height / 2 + 1;
                    using (var pen = new Pen(ThemeManager.Line))
                        g.DrawLine(pen, ts.Width + 10, y, gb.Width - 1, y);
                }
            };
            gb.Invalidate();
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  TABCONTROL -> THANH PHÂN ĐOẠN DẠNG PILL
    // ───────────────────────────────────────────────────────────────────────
    public static class TabSkin
    {
        public static void Attach(TabControl tc)
        {
            if (tc.Parent == null || tc.TabPages.Count == 0) return;
            if (tc.Tag is string && (string)tc.Tag == "tabskin") return;
            tc.Tag = "tabskin";

            var bar = new SegmentBar(tc) { Height = 44 };
            var parent = tc.Parent;
            int idx = parent.Controls.GetChildIndex(tc);

            if (tc.Dock != DockStyle.None)
            {
                // Bọc vào một panel cùng kiểu Dock: thanh pill ở trên, tab ở dưới
                var host = new Panel { Dock = tc.Dock, BackColor = SkinUtil.Backdrop(tc), Bounds = tc.Bounds };
                parent.Controls.Remove(tc);
                parent.Controls.Add(host);
                parent.Controls.SetChildIndex(host, idx);
                bar.Dock = DockStyle.Top;
                tc.Dock = DockStyle.Fill;
                host.Controls.Add(tc);
                host.Controls.Add(bar);
            }
            else
            {
                bar.SetBounds(tc.Left, tc.Top, tc.Width, 44);
                bar.Anchor = (tc.Anchor & ~AnchorStyles.Bottom) | AnchorStyles.Top;
                tc.SetBounds(tc.Left, tc.Top + 48, tc.Width, Math.Max(40, tc.Height - 48));
                parent.Controls.Add(bar);
                parent.Controls.SetChildIndex(bar, idx);
            }

            // Ẩn dải tab gốc
            tc.Appearance = TabAppearance.FlatButtons;
            tc.ItemSize = new Size(0, 1);
            tc.SizeMode = TabSizeMode.Fixed;
            foreach (TabPage p in tc.TabPages)
            {
                p.UseVisualStyleBackColor = false;
                if (p.BackColor == SystemColors.Control || p.BackColor == Color.Transparent) p.BackColor = SkinUtil.Backdrop(tc);
            }
            tc.SelectedIndexChanged += (s, e) => bar.Invalidate();
        }

        private class SegmentBar : Control
        {
            private readonly TabControl _tc;
            private int _hover = -1;
            private readonly List<Rectangle> _rects = new List<Rectangle>();
            public SegmentBar(TabControl tc)
            {
                _tc = tc;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Hand;
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                int h = -1;
                for (int i = 0; i < _rects.Count; i++) if (_rects[i].Contains(e.Location)) h = i;
                if (h != _hover) { _hover = h; Invalidate(); }
                base.OnMouseMove(e);
            }
            protected override void OnMouseLeave(EventArgs e) { _hover = -1; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnMouseClick(MouseEventArgs e)
            {
                for (int i = 0; i < _rects.Count; i++)
                    if (_rects[i].Contains(e.Location) && i < _tc.TabPages.Count) { _tc.SelectedIndex = i; Invalidate(); }
                base.OnMouseClick(e);
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(SkinUtil.Backdrop(this));
                _rects.Clear();
                using (var f = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold))
                {
                    int x = 0;
                    var items = new List<string>();
                    foreach (TabPage p in _tc.TabPages) items.Add(SkinUtil.CleanText(p.Text));
                    int totalW = 8;
                    foreach (var t in items) totalW += TextRenderer.MeasureText(t, f).Width + 28;
                    var outer = new RectangleF(0.5f, 4.5f, Math.Min(Width - 1, totalW) - 1, 35);
                    using (var path = SkinUtil.Round(outer, 10))
                    using (var br = new SolidBrush(Color.FromArgb(236, 238, 245)))
                        g.FillPath(br, path);
                    x = 4;
                    for (int i = 0; i < items.Count; i++)
                    {
                        int w = TextRenderer.MeasureText(items[i], f).Width + 28;
                        var r = new Rectangle(x, 8, w, 28);
                        _rects.Add(r);
                        bool sel = _tc.SelectedIndex == i;
                        if (sel)
                        {
                            using (var path = SkinUtil.Round(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), 8))
                            {
                                using (var br = new SolidBrush(Color.White)) g.FillPath(br, path);
                                using (var pen = new Pen(Color.FromArgb(30, 30, 40, 90))) g.DrawPath(pen, path);
                            }
                        }
                        Color c = sel ? ThemeManager.Primary : (_hover == i ? ThemeManager.Ink : ThemeManager.Muted);
                        TextRenderer.DrawText(g, items[i], f, r, c, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                        x += w;
                    }
                }
            }
        }
    }
}
