using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
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
}
