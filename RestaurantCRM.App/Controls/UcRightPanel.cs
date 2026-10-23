using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>Một dòng trong danh sách "Hoạt động gần đây".</summary>
    public class ActivityItem
    {
        public string Who;       // tên người (dùng làm avatar)
        public string Text;      // nội dung chính
        public DateTime? Time;   // thời điểm
        public string Status;    // trạng thái (vẽ pill nếu nhận diện được)
    }

    /// <summary>
    /// Cột "Hoạt động gần đây": avatar tròn, thời gian tương đối, nội dung, pill trạng thái.
    /// Tự vẽ toàn bộ, cuộn bằng con lăn chuột khi danh sách dài.
    /// </summary>
    public class UcActivityFeed : RoundedPanel
    {
        private List<ActivityItem> _items = new List<ActivityItem>();
        private bool _loading = true;
        private int _scroll;
        private const int ItemH = 66;
        private const int HeaderH = 64;

        public string Title { get; set; } = "Hoạt động gần đây";

        public UcActivityFeed()
        {
            Radius = 14;
            Shadow = 8;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(120, ThemeManager.Line);
            Size = new Size(300, 500);
            DoubleBuffered = true;
        }

        public void SetItems(List<ActivityItem> items)
        {
            _items = items ?? new List<ActivityItem>();
            _loading = false;
            _scroll = 0;
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int contentH = _items.Count * ItemH;
            int viewH = CardRect.Height - HeaderH - 10;
            int maxScroll = Math.Max(0, contentH - viewH);
            _scroll = Math.Max(0, Math.Min(maxScroll, _scroll - e.Delta / 3));
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); Focus(); }

        private static string Ago(DateTime? t)
        {
            if (!t.HasValue) return "";
            var d = DateTime.Now - t.Value;
            if (d.TotalMinutes < 1) return "VỪA XONG";
            if (d.TotalMinutes < 60) return ((int)d.TotalMinutes) + " PHÚT TRƯỚC";
            if (d.TotalHours < 24) return ((int)d.TotalHours) + " GIỜ TRƯỚC";
            if (d.TotalDays < 7) return ((int)d.TotalDays) + " NGÀY TRƯỚC";
            return t.Value.ToString("dd/MM/yyyy");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var card = CardRect;

            using (var f = new Font("Segoe UI", 11.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, Title, f, new Point(card.X + 22, card.Y + 22), ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            using (var f = ThemeManager.Icon(9F))
                TextRenderer.DrawText(g, ThemeManager.Glyph.More, f, new Point(card.Right - 34, card.Y + 25), ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            var view = new Rectangle(card.X + 1, card.Y + HeaderH, card.Width - 2, card.Height - HeaderH - 10);
            if (_loading || _items.Count == 0)
            {
                using (var f = new Font("Segoe UI", 9F))
                    TextRenderer.DrawText(g, _loading ? "Đang tải..." : "Chưa có hoạt động nào", f, view, ThemeManager.Muted,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
                return;
            }

            var state = g.Save();
            g.SetClip(view);
            using (var fTime = new Font("Segoe UI", 6.75F, FontStyle.Bold))
            using (var fText = new Font("Segoe UI", 8.5F))
            using (var fAv = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var fPill = new Font("Segoe UI", 7F, FontStyle.Bold))
            using (var linePen = new Pen(Color.FromArgb(244, 245, 249)))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var it = _items[i];
                    int y = view.Y + i * ItemH - _scroll;
                    if (y + ItemH < view.Y || y > view.Bottom) continue;
                    int x = card.X + 22;

                    // avatar
                    var av = new Rectangle(x, y + 14, 34, 34);
                    using (var br = new SolidBrush(ThemeManager.AvatarColor(it.Who))) g.FillEllipse(br, av);
                    TextRenderer.DrawText(g, ThemeManager.Initials(it.Who), fAv, av, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    // chấm trạng thái nhỏ trên avatar
                    Color sf, sb;
                    bool hasStatus = ThemeManager.TryStatusColors(it.Status, out sf, out sb);
                    if (hasStatus)
                    {
                        using (var br = new SolidBrush(Color.White)) g.FillEllipse(br, av.Right - 11, av.Y - 2, 13, 13);
                        using (var br = new SolidBrush(sf)) g.FillEllipse(br, av.Right - 9, av.Y, 9, 9);
                    }

                    int tx = av.Right + 12;
                    int tw = card.Right - 20 - tx;
                    TextRenderer.DrawText(g, Ago(it.Time), fTime, new Rectangle(tx, y + 12, tw, 14), ThemeManager.Muted,
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, it.Text ?? "", fText, new Rectangle(tx, y + 27, tw, 34), ThemeManager.Ink2,
                        TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    if (i < _items.Count - 1) g.DrawLine(linePen, tx, y + ItemH - 1, card.Right - 20, y + ItemH - 1);
                }
            }
            g.Restore(state);

            // gợi ý cuộn: mờ dần ở đáy
            if (_items.Count * ItemH - _scroll > view.Height)
            {
                var fade = new Rectangle(view.X, view.Bottom - 30, view.Width, 30);
                using (var br = new LinearGradientBrush(fade, Color.FromArgb(0, Color.White), Color.White, LinearGradientMode.Vertical))
                    g.FillRectangle(br, fade);
            }
        }
    }

    /// <summary>Tên cũ — giữ để tương thích.</summary>
    public class UcRightPanel : UcActivityFeed { }
}