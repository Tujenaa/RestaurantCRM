using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    // ═══════════════════════════════════════════════════════════════════════
    //  BỘ CÔNG CỤ GIAO DIỆN DÙNG CHUNG
    //  - ButtonSkin : vẽ lại mọi Button thành nút bo tròn (giữ nguyên Click/Enabled)
    //  - FieldSkin  : ô nhập TextBox / NumericUpDown / ComboBox bo tròn, viền xanh khi focus
    //  - GroupSkin  : GroupBox thành tiêu đề mục gọn gàng (không khung xám)
    //  - TabSkin    : TabControl thành thanh phân đoạn dạng pill
    //  - PillSearch, ComboChip, KpiTile : các khối dựng màn danh sách
    // ═══════════════════════════════════════════════════════════════════════

    internal static class SkinUtil
    {
        /// <summary>Màu nền thực tế phía sau control (xuyên qua các control trong suốt).</summary>
        public static Color Backdrop(Control c)
        {
            var p = c.Parent;
            while (p != null)
            {
                var rp = p as RoundedPanel;
                if (rp != null) return rp.FillColor;
                if (p.BackColor.A == 255) return p.BackColor;
                p = p.Parent;
            }
            return ThemeManager.Bg;
        }

        public static void EnableDoubleBuffer(Control c)
        {
            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
                    null, c, new object[] { true });
            }
            catch { }
        }

        /// <summary>Bỏ emoji / ký hiệu ở đầu chữ (vd "🔄 Làm Mới" -> "Làm Mới").</summary>
        public static string CleanText(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            int i = 0;
            while (i < s.Length)
            {
                char ch = s[i];
                if (char.IsLetterOrDigit(ch)) break;
                i++;
            }
            string r = s.Substring(i).Trim();
            // bỏ emoji cuối chữ (vd "Bán chạy 🔥")
            int j = r.Length;
            while (j > 0 && (char.IsSurrogate(r[j - 1]) || r[j - 1] == '️' || r[j - 1] == ' ')) j--;
            return r.Substring(0, j);
        }

        public static string GlyphFor(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string t = text.ToLowerInvariant();
            string raw = text.TrimStart();
            if (raw.StartsWith("+") || raw.StartsWith("＋") || t.Contains("thêm") || t.StartsWith("tạo") || t.Contains("lập phiếu")) return ThemeManager.Glyph.Add;
            if (t.Contains("sửa") || t.Contains("chỉnh")) return ThemeManager.Glyph.Edit;
            if (t.Contains("xóa") || t.Contains("xoá")) return ThemeManager.Glyph.Delete;
            if (t.Contains("làm mới") || t.Contains("tải lại") || t.Contains("nhập lại") || t.Contains("kinh doanh lại") || t.Contains("khôi phục")) return "\uE72C";
            if (t.Contains("ngừng") || t.Contains("khóa")) return "\uE711";
            if (t.Contains("chi tiết") || t.StartsWith("xem")) return "\uE890";
            if (t.Contains("lưu") || t.Contains("sao lưu")) return ThemeManager.Glyph.Save;
            if (t.Contains("xác nhận") || t.Contains("áp dụng") || t.Contains("cập nhật")) return "\uE8FB";
            if (t.Contains("loại món") || t.Contains("danh mục")) return ThemeManager.Glyph.List;
            if (t.Contains("tìm")) return ThemeManager.Glyph.Search;
            if (t.Contains("xuất") || t.Contains("tải xuống")) return ThemeManager.Glyph.Download;
            if (t.Contains("thống kê") || t.Contains("báo cáo")) return ThemeManager.Glyph.Chart;
            return null;
        }

        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Max(1f, radius * 2);
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ───────────────────────────────────────────────────────────────────────
    //  NÚT BO TRÒN
    // ───────────────────────────────────────────────────────────────────────
    public static class ButtonSkin
    {
        private class State { public bool Hover, Down, KeyFocus, AutoFit, Fitting; public int MinWidth; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Button, State> _states =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Button, State>();

        /// <summary>Kiểu nút: Primary (xanh đặc), Secondary (trắng viền), Soft (nền nhạt), Danger (đỏ nhạt), Ghost (không nền).</summary>
        public enum Kind { Primary, Secondary, Soft, Danger, Ghost }

        public static void SetKind(Button b, Kind kind)
        {
            b.UseVisualStyleBackColor = false;   // để bộ style chung không coi là nút trung tính và đổi về nền trắng
            b.FlatStyle = FlatStyle.Flat;
            switch (kind)
            {
                case Kind.Primary:
                    b.BackColor = ThemeManager.Primary; b.ForeColor = Color.White; b.FlatAppearance.BorderSize = 0; break;
                case Kind.Secondary:
                    b.BackColor = Color.White; b.ForeColor = ThemeManager.Ink2; b.FlatAppearance.BorderSize = 1; b.FlatAppearance.BorderColor = ThemeManager.Line; break;
                case Kind.Soft:
                    b.BackColor = ThemeManager.PrimarySoft; b.ForeColor = ThemeManager.Primary; b.FlatAppearance.BorderSize = 0; break;
                case Kind.Danger:
                    b.BackColor = ThemeManager.RedSoft; b.ForeColor = ThemeManager.Red; b.FlatAppearance.BorderSize = 0; break;
                case Kind.Ghost:
                    b.BackColor = Color.White; b.ForeColor = ThemeManager.Ink2; b.FlatAppearance.BorderSize = 0; break;
            }
            Attach(b);
            b.Invalidate();
        }

        public static bool CanSkin(Button b)
        {
            if (b == null || b.Image != null || b.BackgroundImage != null) return false;
            if (b.Tag is string && (string)b.Tag == UiStyler.NoStyleTag) return false;
            if (b.BackColor == Color.Transparent) return false;
            if (b.Text != null && b.Text.Contains("\n")) return false;
            return true;
        }

        /// <summary>
        /// Bật chế độ tự giãn: nút luôn đủ rộng cho chữ thật đang vẽ (đúng cả khi Windows phóng to 125% / 150%).
        /// </summary>
        public static void EnableAutoFit(Button b, int minWidth)
        {
            Attach(b);
            State st;
            if (!_states.TryGetValue(b, out st)) return;
            st.MinWidth = minWidth;
            if (st.AutoFit) { Fit(b, st); return; }
            st.AutoFit = true;
            b.FontChanged += (s, e) => Fit(b, st);
            b.TextChanged += (s, e) => Fit(b, st);
            b.HandleCreated += (s, e) => Fit(b, st);
            b.ParentChanged += (s, e) => Fit(b, st);
            Fit(b, st);
        }

        private static int Needed(Button b, out bool withIcon)
        {
            string raw = (b.Text ?? "").Replace("&&", "&");
            string text = SkinUtil.CleanText(raw);
            if (text.Length == 0) text = raw.Trim();
            withIcon = SkinUtil.GlyphFor(b.Text) != null && b.Height >= 28;
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            int tw = TextRenderer.MeasureText(text, b.Font, Size.Empty, flags).Width;
            return tw + (withIcon ? 24 : 0) + 36;   // 18px lề mỗi bên
        }

        private static void Fit(Button b, State st)
        {
            if (st.Fitting || b.IsDisposed) return;
            bool icon;
            int w = Math.Max(st.MinWidth, Needed(b, out icon));
            if (w == b.Width) return;
            st.Fitting = true;
            try { b.Width = w; } finally { st.Fitting = false; }
        }

        public static void Attach(Button b)
        {
            State st;
            if (_states.TryGetValue(b, out st)) return;
            st = new State();
            _states.Add(b, st);
            b.FlatStyle = FlatStyle.Flat;
            b.UseMnemonic = false;
            b.Cursor = Cursors.Hand;
            SkinUtil.EnableDoubleBuffer(b);
            b.MouseEnter += (s, e) => { st.Hover = true; b.Invalidate(); };
            b.MouseLeave += (s, e) => { st.Hover = false; st.Down = false; b.Invalidate(); };
            b.MouseDown += (s, e) => { st.Down = true; b.Invalidate(); };
            b.MouseUp += (s, e) => { st.Down = false; b.Invalidate(); };
            b.EnabledChanged += (s, e) => b.Invalidate();
            // Viền focus chỉ hiện khi di chuyển bằng phím Tab (không hiện khi bấm chuột)
            b.KeyUp += (s, e) => { if (e.KeyCode == Keys.Tab) { st.KeyFocus = true; b.Invalidate(); } };
            b.Leave += (s, e) => { st.KeyFocus = false; b.Invalidate(); };
            b.TextChanged += (s, e) => b.Invalidate();
            b.Paint += (s, e) => Paint(b, st, e.Graphics);
        }

        private static void Paint(Button b, State st, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(SkinUtil.Backdrop(b));
            var rc = new RectangleF(0.5f, 0.5f, b.Width - 1.5f, b.Height - 1.5f);
            if (rc.Width < 4 || rc.Height < 4) return;
            float radius = Math.Min(9f, rc.Height / 2f);

            Color bg = b.BackColor.A == 0 ? Color.White : b.BackColor;
            bool light = bg.GetBrightness() > 0.9f;
            Color fg = b.ForeColor;
            Color border = Color.Empty;
            if (b.FlatAppearance.BorderSize > 0)
                border = b.FlatAppearance.BorderColor.IsEmpty ? ThemeManager.Line : b.FlatAppearance.BorderColor;

            if (!b.Enabled)
            {
                bg = ThemeManager.Surface2; fg = Color.FromArgb(178, 183, 199); border = ThemeManager.Line;
            }
            else if (st.Down)
            {
                bg = light ? ThemeManager.Blend(bg, ThemeManager.Primary, 0.12f) : ThemeManager.Darken(bg, 0.18f);
            }
            else if (st.Hover)
            {
                if (light)
                {
                    bg = bg.ToArgb() == Color.White.ToArgb() ? ThemeManager.PrimarySoft : ThemeManager.Darken(bg, 0.04f);
                    if (!border.IsEmpty && border.ToArgb() == ThemeManager.Line.ToArgb()) border = Color.FromArgb(150, ThemeManager.Primary);
                    if (fg.ToArgb() == ThemeManager.Ink2.ToArgb() || fg.ToArgb() == ThemeManager.Ink.ToArgb()) fg = ThemeManager.Primary;
                }
                else bg = ThemeManager.Darken(bg, 0.1f);
            }

            using (var path = SkinUtil.Round(rc, radius))
            {
                using (var br = new SolidBrush(bg)) g.FillPath(br, path);
                if (!border.IsEmpty)
                    using (var pen = new Pen(border, 1f)) g.DrawPath(pen, path);
                if (b.Focused && !st.Hover && st.KeyFocus)
                    using (var pen = new Pen(Color.FromArgb(110, ThemeManager.Primary), 1.6f)) g.DrawPath(pen, path);
            }

            string rawText = (b.Text ?? "").Replace("&&", "&");
            string text = SkinUtil.CleanText(rawText);
            // Nút chỉ có ký hiệu ("+", "−", "✕"...): giữ nguyên ký hiệu, không thêm icon
            bool symbolOnly = text.Length == 0;
            if (symbolOnly) text = rawText.Trim();
            string glyph = (!symbolOnly && b.Height >= 28 && b.Width >= 70) ? SkinUtil.GlyphFor(b.Text) : null;
            var font = b.Font;
            if (font.Name == "Microsoft Sans Serif") font = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            var ts = TextRenderer.MeasureText(text, font, Size.Empty, flags);
            int iconW = glyph != null && text.Length > 0 ? 22 : (glyph != null ? 16 : 0);
            // Không đủ chỗ: bỏ icon trước, chỉ cắt chữ khi thật sự hết chỗ
            if (glyph != null && ts.Width + iconW + 16 > b.Width) { glyph = null; iconW = 0; }
            int total = ts.Width + iconW;
            // Nút tự giãn mà vẫn thiếu chỗ (vd font lớn hơn dự kiến) -> nới rộng ở lượt vẽ sau
            if (st.AutoFit && !st.Fitting && total + 28 > b.Width)
                b.BeginInvoke((Action)(() => Fit(b, st)));
            int pad = 12;
            int x;
            var ha = b.TextAlign;
            if (ha == ContentAlignment.MiddleLeft || ha == ContentAlignment.TopLeft || ha == ContentAlignment.BottomLeft) x = pad;
            else if (ha == ContentAlignment.MiddleRight || ha == ContentAlignment.TopRight || ha == ContentAlignment.BottomRight) x = b.Width - pad - total;
            else x = (b.Width - total) / 2;
            x = Math.Max(6, x);
            if (glyph != null)
            {
                using (var fi = ThemeManager.Icon(Math.Max(8f, font.SizeInPoints)))
                    TextRenderer.DrawText(g, glyph, fi, new Rectangle(x, 0, 16, b.Height), fg,
                        TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            }
            // Vùng chữ kéo tới sát mép phải nút (không dùng "..." để chữ không bao giờ bị cắt oan)
            TextRenderer.DrawText(g, text, font, new Rectangle(x + iconW, 0, Math.Max(10, b.Width - x - iconW), b.Height), fg,
                flags | TextFormatFlags.VerticalCenter);
        }
    }

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
            g.SmoothingMode = SmoothingMode.AntiAlias;
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

    /// <summary>Panel / PictureBox có viền FixedSingle (đen) -> viền mảnh màu xám nhạt bo góc.</summary>
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
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

    // ───────────────────────────────────────────────────────────────────────
    //  THẺ KPI GỌN (lấy dữ liệu từ một Label nguồn: "Tiêu đề: giá trị")
    // ───────────────────────────────────────────────────────────────────────
    public class KpiTile : RoundedPanel
    {
        private string _caption = "";
        private string _value = "";
        private Color _accent;
        private readonly string _fixedCaption;

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
        private bool _accentAuto;

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
            g.SmoothingMode = SmoothingMode.AntiAlias;
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