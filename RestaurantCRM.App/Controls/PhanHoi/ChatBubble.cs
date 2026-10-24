using System;
using System.Drawing;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Controls;
using RestaurantCRM.AdminApp.Helpers;

namespace RestaurantCRM.AdminApp.Forms.PhanHoi
{
    /// <summary>Bong bóng hội thoại: khách hàng bên trái (xám), nhân viên bên phải (xanh nhạt) kèm nút xóa.</summary>
    public class ChatBubble : Control
    {
        private const int Pad = 12, MaxRatio = 86;
        private Rectangle _bubble, _del;
        private bool _hoverDel;

        public string Author { get; set; }
        public DateTime? Time { get; set; }
        public string Body { get; set; }
        public bool Mine { get; set; }
        public bool CanDelete { get; set; }
        public event EventHandler DeleteClicked;

        public ChatBubble()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;
            Font = new Font(ThemeManager.FontFamily, 9.5F);
        }

        private static readonly TextFormatFlags Wrap = TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;

        public void LayoutFor(int width)
        {
            int maxW = Math.Max(120, width * MaxRatio / 100);
            Size sz;
            using (var g = CreateGraphics())
                sz = TextRenderer.MeasureText(g, Body ?? "", Font, new Size(maxW - Pad * 2, int.MaxValue), Wrap);
            int metaW;
            using (var fMeta = new Font(ThemeManager.FontFamily, 8F))
                metaW = TextRenderer.MeasureText(MetaText(), fMeta).Width + (CanDelete ? 40 : 0);
            int bw = Math.Min(maxW, Math.Max(sz.Width + Pad * 2, metaW + 4));
            int bh = sz.Height + Pad * 2;
            _bubble = new Rectangle(Mine ? width - bw - 1 : 0, 20, bw, bh);
            _del = CanDelete ? new Rectangle(_bubble.Right - 30, 0, 30, 18) : Rectangle.Empty;
            Size = new Size(width, 20 + bh + 1);
            Invalidate();
        }

        private string MetaText()
        {
            return (Author ?? "") + (Time.HasValue ? "  ·  " + Time.Value.ToString("HH:mm dd/MM/yyyy") : "");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var f = new Font(ThemeManager.FontFamily, 8F))
            {
                var flags = TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | (Mine ? TextFormatFlags.Right : TextFormatFlags.Left);
                var metaRect = Mine
                    ? new Rectangle(0, 2, _bubble.Right - (CanDelete ? 36 : 0), 16)
                    : new Rectangle(_bubble.Left, 2, Width - _bubble.Left, 16);
                TextRenderer.DrawText(g, MetaText(), f, metaRect, ThemeManager.Muted, flags);
                if (CanDelete)
                    TextRenderer.DrawText(g, "Xóa", f, _del, _hoverDel ? ThemeManager.Red : ThemeManager.Muted,
                        TextFormatFlags.Right | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
            }
            using (var path = SkinUtil.Round(_bubble, 12))
            using (var br = new SolidBrush(Mine ? ThemeManager.PrimarySoft : ThemeManager.Surface2))
                g.FillPath(br, path);
            var tr = Rectangle.Inflate(_bubble, -Pad, -Pad);
            TextRenderer.DrawText(g, Body ?? "", Font, tr, ThemeManager.Ink, Wrap);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            bool h = CanDelete && _del.Contains(e.Location);
            if (h != _hoverDel) { _hoverDel = h; Cursor = h ? Cursors.Hand : Cursors.Default; Invalidate(_del); }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (_hoverDel) { _hoverDel = false; Cursor = Cursors.Default; Invalidate(_del); }
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (CanDelete && _del.Contains(e.Location)) DeleteClicked?.Invoke(this, EventArgs.Empty);
            base.OnMouseClick(e);
        }
    }
}
