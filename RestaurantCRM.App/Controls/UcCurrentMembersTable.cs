using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Thẻ xếp hạng (ví dụ: Món bán chạy): số thứ tự, tên, thanh tỉ lệ mảnh và giá trị.
    /// </summary>
    public class UcTopList : RoundedPanel
    {
        private List<KeyValuePair<string, double>> _items = new List<KeyValuePair<string, double>>();
        private bool _loading = true;

        public string Title { get; set; } = "Món bán chạy";
        public string Subtitle { get; set; } = "XẾP HẠNG THEO SỐ LƯỢNG";
        public string Unit { get; set; } = "phần";

        public UcTopList()
        {
            Radius = 14;
            Shadow = 8;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(120, ThemeManager.Line);
            Size = new Size(360, 260);
            DoubleBuffered = true;
        }

        public void SetItems(List<KeyValuePair<string, double>> items)
        {
            _items = items ?? new List<KeyValuePair<string, double>>();
            _loading = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var card = CardRect;
            int x = card.X + 24;

            using (var f = new Font("Segoe UI", 11.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, Title, f, new Point(x, card.Y + 20), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
                TextRenderer.DrawText(g, Subtitle, f, new Point(x, card.Y + 44), ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            var area = new Rectangle(x, card.Y + 70, card.Width - 48, card.Height - 84);
            if (_loading || _items.Count == 0)
            {
                using (var f = new Font("Segoe UI", 9F))
                    TextRenderer.DrawText(g, _loading ? "Đang tải..." : "Chưa có dữ liệu", f, area, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            double max = 0;
            foreach (var it in _items) if (it.Value > max) max = it.Value;
            if (max <= 0) max = 1;

            int rowH = Math.Max(30, Math.Min(42, area.Height / Math.Max(1, _items.Count)));
            using (var fNum = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var fName = new Font("Segoe UI", 9F))
            using (var fVal = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    int y = area.Y + i * rowH;
                    if (y + rowH > card.Bottom - 6) break;
                    var it = _items[i];

                    // số thứ tự trong vòng tròn nhỏ
                    var num = new Rectangle(x, y + 2, 22, 22);
                    bool top = i == 0;
                    using (var br = new SolidBrush(top ? ThemeManager.Primary : ThemeManager.PrimarySoft)) g.FillEllipse(br, num);
                    TextRenderer.DrawText(g, (i + 1).ToString(), fNum, num, top ? Color.White : ThemeManager.Primary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    string val = it.Value.ToString("N0") + " " + Unit;
                    var vs = TextRenderer.MeasureText(g, val, fVal, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    int nx = num.Right + 12;
                    TextRenderer.DrawText(g, it.Key ?? "", fName, new Rectangle(nx, y + 1, area.Right - nx - vs.Width - 10, 18),
                        ThemeManager.Ink, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, val, fVal, new Point(area.Right - vs.Width, y + 2), ThemeManager.Ink2, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    // thanh tỉ lệ mảnh
                    var track = new Rectangle(nx, y + 21, area.Right - nx, 5);
                    using (var path = ThemeManager.RoundedRect(track, 2))
                    using (var br = new SolidBrush(Color.FromArgb(242, 243, 248)))
                        g.FillPath(br, path);
                    int fw = Math.Max(6, (int)(track.Width * it.Value / max));
                    using (var path = ThemeManager.RoundedRect(new Rectangle(track.X, track.Y, fw, track.Height), 2))
                    using (var br = new SolidBrush(top ? ThemeManager.Primary : Color.FromArgb(150, ThemeManager.Primary)))
                        g.FillPath(br, path);
                }
            }
        }
    }

    /// <summary>Tên cũ — giữ để tương thích.</summary>
    public class UcTopMonAnTable : UcTopList { }

    /// <summary>Tên cũ — giữ để tương thích.</summary>
    public class UcCurrentMembersTable : UcTopList { }
}