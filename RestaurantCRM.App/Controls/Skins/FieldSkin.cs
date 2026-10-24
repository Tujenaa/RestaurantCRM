using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    // ───────────────────────────────────────────────────────────────────────
    //  Ô NHẬP BO TRÒN (TextBox / NumericUpDown / ComboBox)
    // ───────────────────────────────────────────────────────────────────────
    public static class FieldSkin
    {
        private class FieldInfo
        {
            public Control Ctl; public bool Focus;
            public int L, T, R, B;   // khoảng lệch khung so với control (để khung đi theo khi control co giãn / di chuyển)
            public Rectangle Box { get { var b = Ctl.Bounds; return Rectangle.FromLTRB(b.Left + L, b.Top + T, b.Right + R, b.Bottom + B); } }
        }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, List<FieldInfo>> _byParent =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Control, List<FieldInfo>>();
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, object> _done =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Control, object>();

        /// <summary>Áp kiểu ô nhập cho mọi TextBox/NumericUpDown/ComboBox bên trong root.</summary>
        public static void ApplyAll(Control root)
        {
            if (root == null) return;
            var list = new List<Control>();
            Collect(root, list);
            foreach (var c in list) Apply(c);
        }

        private static void Collect(Control c, List<Control> list)
        {
            foreach (Control k in c.Controls)
            {
                if (k.Tag is string && (string)k.Tag == UiStyler.NoStyleTag) continue;
                if (k is TextBox || k is NumericUpDown || k is ComboBox || k is DateTimePicker) list.Add(k);
                else if (!(k is DataGridView) && !(k is UpDownBase)) Collect(k, list);
            }
        }

        public static void Apply(Control c)
        {
            object dummy;
            if (c == null || c.Parent == null || _done.TryGetValue(c, out dummy)) return;
            if (c is TextBox && ((TextBox)c).BorderStyle == BorderStyle.None) return; // đã tự xử lý
            if (!c.Visible && c.Parent.Visible) { /* vẫn áp để khi hiện lên đã đẹp */ }
            _done.Add(c, null);

            var parent = c.Parent;
            var orig = c.Bounds;
            bool multiline = c is TextBox && ((TextBox)c).Multiline;

            // khoảng trống trên/dưới để nới khung, không đè lên control khác
            int v = multiline ? 4 : 6;
            foreach (Control sib in parent.Controls)
            {
                if (sib == c || !sib.Visible) continue;
                var sb = sib.Bounds;
                if (sb.Right <= orig.Left || sb.Left >= orig.Right) continue;
                if (sb.Bottom <= orig.Top) v = Math.Min(v, Math.Max(1, orig.Top - sb.Bottom - 2));
                else if (sb.Top >= orig.Bottom) v = Math.Min(v, Math.Max(1, sb.Top - orig.Bottom - 2));
            }
            var box = new Rectangle(orig.X, orig.Y - v, orig.Width, orig.Height + v * 2);
            int inset = 10;

            if (c is TextBox)
            {
                var tb = (TextBox)c;
                tb.BorderStyle = BorderStyle.None;
                if (tb.BackColor == SystemColors.Window || tb.BackColor.GetBrightness() > 0.9f || tb.ReadOnly)
                    tb.BackColor = tb.ReadOnly ? ThemeManager.Surface2 : Color.White;
                if (multiline)
                    tb.SetBounds(orig.X + inset, orig.Y + 2, orig.Width - inset * 2, orig.Height - 4);
                else
                    tb.SetBounds(orig.X + inset, orig.Y + (orig.Height - tb.PreferredHeight) / 2 + 1, orig.Width - inset * 2, tb.Height);
            }
            else if (c is NumericUpDown)
            {
                var nu = (NumericUpDown)c;
                nu.BorderStyle = BorderStyle.None;
                nu.BackColor = Color.White;
                nu.SetBounds(orig.X + inset, orig.Y + (orig.Height - nu.PreferredHeight) / 2 + 1, orig.Width - inset - 4, nu.Height);
            }
            else if (c is ComboBox)
            {
                var cb = (ComboBox)c;
                cb.FlatStyle = FlatStyle.Flat;
                cb.BackColor = Color.White;
                cb.SetBounds(orig.X + 4, orig.Y, orig.Width - 6, orig.Height);
                ComboFrame.Attach(cb);
            }
            else if (c is DateTimePicker)
            {
                var dp = (DateTimePicker)c;
                dp.SetBounds(orig.X + 4, orig.Y, orig.Width - 6, orig.Height);
                DateFrame.Attach(dp);
            }

            List<FieldInfo> fields;
            if (!_byParent.TryGetValue(parent, out fields))
            {
                fields = new List<FieldInfo>();
                _byParent.Add(parent, fields);
                var captured = fields;
                parent.Paint += (s, e) => PaintFields((Control)s, captured, e.Graphics);
            }
            var nb = c.Bounds;
            var fi = new FieldInfo { Ctl = c, L = box.Left - nb.Left, T = box.Top - nb.Top, R = box.Right - nb.Right, B = box.Bottom - nb.Bottom };
            fields.Add(fi);
            c.Enter += (s, e) => { fi.Focus = true; parent.Invalidate(Rectangle.Inflate(fi.Box, 2, 2)); };
            c.Leave += (s, e) => { fi.Focus = false; parent.Invalidate(Rectangle.Inflate(fi.Box, 2, 2)); };
            c.VisibleChanged += (s, e) => parent.Invalidate();
            c.EnabledChanged += (s, e) => parent.Invalidate(Rectangle.Inflate(fi.Box, 2, 2));
            c.LocationChanged += (s, e) => parent.Invalidate();
            c.SizeChanged += (s, e) => parent.Invalidate();
            parent.Invalidate();
        }

        private static void PaintFields(Control parent, List<FieldInfo> fields, Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var f in fields)
            {
                if (f.Ctl.IsDisposed || !f.Ctl.Visible || f.Ctl.Parent != parent) continue;
                var r = new RectangleF(f.Box.X + 0.5f, f.Box.Y + 0.5f, f.Box.Width - 1, f.Box.Height - 1);
                Color fill = f.Ctl.BackColor;
                if (!f.Ctl.Enabled) fill = ThemeManager.Surface2;
                using (var path = SkinUtil.Round(r, Math.Min(9f, r.Height / 2f)))
                {
                    using (var br = new SolidBrush(fill)) g.FillPath(br, path);
                    using (var pen = new Pen(f.Focus ? ThemeManager.Primary : Color.FromArgb(222, 225, 234), f.Focus ? 1.6f : 1f)) g.DrawPath(pen, path);
                }
            }
        }
    }

    /// <summary>Che viền vuông mặc định của ComboBox Flat và vẽ lại mũi tên dạng chevron.</summary>
    internal class ComboFrame : NativeWindow
    {
        private const int WM_PAINT = 0x000F;
        private readonly ComboBox _cb;
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ComboBox, ComboFrame> _all =
            new System.Runtime.CompilerServices.ConditionalWeakTable<ComboBox, ComboFrame>();

        public static void Attach(ComboBox cb)
        {
            ComboFrame f;
            if (_all.TryGetValue(cb, out f)) return;
            f = new ComboFrame(cb);
            _all.Add(cb, f);
        }

        private ComboFrame(ComboBox cb)
        {
            _cb = cb;
            if (cb.IsHandleCreated) AssignHandle(cb.Handle);
            cb.HandleCreated += (s, e) => { try { AssignHandle(cb.Handle); } catch { } };
            cb.HandleDestroyed += (s, e) => ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_PAINT || _cb.IsDisposed) return;
            try
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    var bg = _cb.Enabled ? _cb.BackColor : ThemeManager.Surface2;
                    int w = _cb.Width, h = _cb.Height;
                    using (var pen = new Pen(bg, 2)) g.DrawRectangle(pen, 1, 1, w - 2, h - 2);
                    using (var pen = new Pen(bg, 1)) g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
                    var arrow = new Rectangle(w - 20, 1, 19, h - 2);
                    using (var br = new SolidBrush(bg)) g.FillRectangle(br, arrow);
                    using (var f = ThemeManager.Icon(7F))
                        TextRenderer.DrawText(g, ThemeManager.Glyph.ChevronDown, f, arrow, ThemeManager.Muted,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
            catch { }
        }
    }

    /// <summary>Che khung 3D của DateTimePicker và thay nút thả xuống bằng biểu tượng lịch.</summary>
    internal class DateFrame : NativeWindow
    {
        private const int WM_PAINT = 0x000F;
        private readonly DateTimePicker _dp;
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DateTimePicker, DateFrame> _all =
            new System.Runtime.CompilerServices.ConditionalWeakTable<DateTimePicker, DateFrame>();

        public static void Attach(DateTimePicker dp)
        {
            DateFrame f;
            if (_all.TryGetValue(dp, out f)) return;
            f = new DateFrame(dp);
            _all.Add(dp, f);
        }

        private DateFrame(DateTimePicker dp)
        {
            _dp = dp;
            if (dp.IsHandleCreated) AssignHandle(dp.Handle);
            dp.HandleCreated += (s, e) => { try { AssignHandle(dp.Handle); } catch { } };
            dp.HandleDestroyed += (s, e) => ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != WM_PAINT || _dp.IsDisposed || _dp.ShowUpDown) return;
            try
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    var bg = _dp.Enabled ? Color.White : ThemeManager.Surface2;
                    int w = _dp.Width, h = _dp.Height;
                    using (var pen = new Pen(bg, 2)) g.DrawRectangle(pen, 1, 1, w - 2, h - 2);
                    using (var pen = new Pen(bg, 1)) g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
                    int btnW = Math.Max(34, SystemInformation.VerticalScrollBarWidth + 14);
                    var btn = new Rectangle(w - btnW, 0, btnW, h);
                    using (var br = new SolidBrush(bg)) g.FillRectangle(br, btn);
                    using (var f = ThemeManager.Icon(9F))
                        TextRenderer.DrawText(g, ThemeManager.Glyph.Calendar, f, btn, ThemeManager.Muted,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
            catch { }
        }
    }
}
