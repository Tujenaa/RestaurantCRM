using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.DTOs.PhanHoi;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls.PhanHoi
{
    /// <summary>
    /// Khối "Đánh giá từng món" trong khung chi tiết phản hồi: tên món, số sao, nhận xét.
    /// Món bị chấm thấp (≤ 2★) được tô đỏ nhạt để admin thấy ngay.
    /// </summary>
    public class DanhGiaMonList : Control
    {
        private const int Pad = 12, Gap = 6, CardPad = 10;
        private const TextFormatFlags Wrap = TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl;
        private const TextFormatFlags One = TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;

        private readonly List<DanhGiaMonDto> _items;
        private readonly Font _fHead = new Font(ThemeManager.FontFamily, 8F, FontStyle.Bold);
        private readonly Font _fName = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
        private readonly Font _fStar = new Font(ThemeManager.FontFamily, 9F, FontStyle.Bold);
        private readonly Font _fText = new Font(ThemeManager.FontFamily, 8.5F);
        private readonly List<Rectangle> _cards = new List<Rectangle>();

        public DanhGiaMonList(List<DanhGiaMonDto> items)
        {
            _items = items ?? new List<DanhGiaMonDto>();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
        }

        public static bool BiChe(DanhGiaMonDto m) { return m != null && m.SoSao > 0 && m.SoSao <= 2; }

        private string HeadText()
        {
            int che = 0;
            foreach (var m in _items) if (BiChe(m)) che++;
            return "ĐÁNH GIÁ TỪNG MÓN (" + _items.Count + " MÓN" + (che > 0 ? " · " + che + " MÓN BỊ CHÊ" : "") + ")";
        }

        /// <summary>Đặt chiều rộng và tính lại chiều cao theo nội dung.</summary>
        public void LayoutFor(int width)
        {
            Width = Math.Max(120, width);
            _cards.Clear();
            int y = TextRenderer.MeasureText(HeadText(), _fHead).Height + Gap;
            int inner = Width - CardPad * 2;
            foreach (var m in _items)
            {
                int h = CardPad + _fName.Height + 2;
                if (!string.IsNullOrWhiteSpace(m.NoiDung))
                    h += 4 + TextRenderer.MeasureText(m.NoiDung.Trim(), _fText, new Size(inner, int.MaxValue), Wrap).Height;
                h += CardPad;
                _cards.Add(new Rectangle(0, y, Width - 1, h));
                y += h + Gap;
            }
            Height = y + 2;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);
            TextRenderer.DrawText(g, HeadText(), _fHead, new Point(0, 0), ThemeManager.Muted, One);

            for (int i = 0; i < _items.Count && i < _cards.Count; i++)
            {
                var m = _items[i];
                var r = _cards[i];
                bool che = BiChe(m);
                using (var path = SkinUtil.Round(new RectangleF(r.X + 0.5f, r.Y + 0.5f, r.Width - 1, r.Height - 1), 8f))
                {
                    using (var br = new SolidBrush(che ? ThemeManager.RedSoft : ThemeManager.Surface2)) g.FillPath(br, path);
                    using (var pen = new Pen(che ? Color.FromArgb(246, 196, 202) : ThemeManager.Line)) g.DrawPath(pen, path);
                }

                int x = r.X + CardPad, top = r.Y + CardPad, inner = r.Width - CardPad * 2;
                int sao = Math.Max(0, Math.Min(5, m.SoSao));
                string stars = new string('★', sao) + new string('☆', 5 - sao) + "  " + sao + "/5";
                // Ký tự ★ vẽ bằng font dự phòng nên rộng hơn số đo: chừa thêm chỗ, căn phải, không cắt "…"
                int starW = (int)(TextRenderer.MeasureText(g, stars, _fStar).Width * 1.25f) + 12;
                TextRenderer.DrawText(g, stars, _fStar, new Rectangle(r.Right - CardPad - starW, top, starW, _fName.Height + 4),
                    che ? ThemeManager.Red : ThemeManager.Amber, TextFormatFlags.Right | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
                string name = (che ? "⚠ " : "") + (string.IsNullOrWhiteSpace(m.TenMon) ? m.MaMon : m.TenMon);
                TextRenderer.DrawText(g, name, _fName, new Rectangle(x, top, inner - starW - 8, _fName.Height), che ? ThemeManager.Red : ThemeManager.Ink, One);

                if (!string.IsNullOrWhiteSpace(m.NoiDung))
                {
                    int ty = top + _fName.Height + 6;
                    TextRenderer.DrawText(g, m.NoiDung.Trim(), _fText, new Rectangle(x, ty, inner, r.Bottom - CardPad - ty + 2), ThemeManager.Ink2, Wrap);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _fHead.Dispose(); _fName.Dispose(); _fStar.Dispose(); _fText.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
