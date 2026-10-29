using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using RestaurantCRM.AdminApp.Helpers;
using RestaurantCRM.AdminApp.Models;

namespace RestaurantCRM.AdminApp.Controls
{
    /// <summary>
    /// Thẻ trực quan hóa cơ cấu độ tuổi & giới tính khách hàng.
    /// Tự vẽ GDI+ sắc nét, bo góc và chống nhấp nháy.
    /// </summary>
    public class UcCustomerDemographics : RoundedPanel
    {
        private List<NhomDoTuoiItemDto> _doTuoi = new List<NhomDoTuoiItemDto>();
        private List<GioiTinhItemDto> _gioiTinh = new List<GioiTinhItemDto>();
        private int _tongKhach = 0;
        private double _doTuoiTb = 0;
        private bool _loading = true;

        public string Title { get; set; } = "Cơ cấu độ tuổi & Giới tính";
        public string Subtitle { get; set; } = "PHÂN BỐ THEO NHÓM TUỔI & NHÂN KHẨU HỌC";

        // Bảng màu cho từng nhóm độ tuổi
        private static readonly Color[] AgePalette = new Color[]
        {
            ColorTranslator.FromHtml("#3BA4F5"), // < 18: Sky
            ColorTranslator.FromHtml("#B8461B"), // 18 - 24: Primary (Cam đất)
            ColorTranslator.FromHtml("#14A99A"), // 25 - 34: Teal (Xanh ngọc)
            ColorTranslator.FromHtml("#E99A2C"), // 35 - 50: Amber (Cam vàng)
            ColorTranslator.FromHtml("#7B6CF6"), // > 50: Violet (Tím)
            ColorTranslator.FromHtml("#A2AABF")  // Chưa rõ: Xám
        };

        public UcCustomerDemographics()
        {
            Radius = 14;
            Shadow = 8;
            FillColor = ThemeManager.Paper;
            BorderColor = Color.FromArgb(120, ThemeManager.Line);
            Size = new Size(380, 360);
            DoubleBuffered = true;
        }

        public void SetData(PhanTichKhachHangDto data)
        {
            if (data == null)
            {
                _doTuoi = new List<NhomDoTuoiItemDto>();
                _gioiTinh = new List<GioiTinhItemDto>();
                _tongKhach = 0;
                _doTuoiTb = 0;
            }
            else
            {
                _doTuoi = data.DoTuoi ?? new List<NhomDoTuoiItemDto>();
                _gioiTinh = data.GioiTinh ?? new List<GioiTinhItemDto>();
                _tongKhach = data.TongKhachHang;
                _doTuoiTb = data.DoTuoiTrungBinh;
            }

            _loading = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var card = CardRect;
            int x = card.X + 22;
            int w = card.Width - 44;

            // 1. Tiêu đề
            using (var f = new Font("Segoe UI", 11.5F, FontStyle.Bold))
                TextRenderer.DrawText(g, Title, f, new Point(x, card.Y + 18), ThemeManager.Ink,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            using (var f = new Font("Segoe UI", 7F, FontStyle.Bold))
                TextRenderer.DrawText(g, Subtitle, f, new Point(x, card.Y + 42), ThemeManager.Muted,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

            // Badge Tuổi TB góc phải
            if (!_loading && _doTuoiTb > 0)
            {
                string tbText = $"Tuổi TB: {_doTuoiTb:0.0}";
                using (var fTb = new Font("Segoe UI", 8F, FontStyle.Bold))
                {
                    var sz = TextRenderer.MeasureText(g, tbText, fTb, Size.Empty, TextFormatFlags.NoPadding);
                    var badgeRect = new Rectangle(card.Right - sz.Width - 32, card.Y + 18, sz.Width + 14, 22);
                    using (var path = ThemeManager.RoundedRect(badgeRect, 6))
                    using (var br = new SolidBrush(ThemeManager.PrimarySoft))
                        g.FillPath(br, path);
                    TextRenderer.DrawText(g, tbText, fTb, badgeRect, ThemeManager.Primary,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }

            var area = new Rectangle(x, card.Y + 68, w, card.Height - 118);

            if (_loading || _doTuoi.Count == 0)
            {
                using (var f = new Font("Segoe UI", 9F))
                    TextRenderer.DrawText(g, _loading ? "Đang tải dữ liệu độ tuổi..." : "Chưa có dữ liệu khách hàng", f,
                        area, ThemeManager.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            // 2. Danh sách thanh tỷ lệ theo nhóm tuổi
            int count = _doTuoi.Count;
            int rowH = Math.Max(32, Math.Min(40, area.Height / Math.Max(1, count)));

            using (var fNhom = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var fMoTa = new Font("Segoe UI", 7.5F))
            using (var fPct = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            using (var fCount = new Font("Segoe UI", 7.5F))
            {
                for (int i = 0; i < count; i++)
                {
                    int y = area.Y + i * rowH;
                    if (y + rowH > area.Bottom) break;
                    var it = _doTuoi[i];
                    Color col = AgePalette[i % AgePalette.Length];

                    // Tên nhóm (Ví dụ: "18 - 24")
                    int labelW = 54;
                    TextRenderer.DrawText(g, it.TenNhom, fNhom, new Rectangle(x, y + 1, labelW, 18),
                        ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    // Giá trị % & Số lượng bên phải
                    string pctText = $"{it.TyLe:0.0}%";
                    string cntText = $"({it.SoLuong})";
                    var szPct = TextRenderer.MeasureText(g, pctText, fPct, Size.Empty, TextFormatFlags.NoPadding);
                    var szCnt = TextRenderer.MeasureText(g, cntText, fCount, Size.Empty, TextFormatFlags.NoPadding);
                    int rightW = szPct.Width + szCnt.Width + 6;

                    TextRenderer.DrawText(g, pctText, fPct, new Point(area.Right - rightW, y + 1),
                        ThemeManager.Ink, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    TextRenderer.DrawText(g, cntText, fCount, new Point(area.Right - szCnt.Width, y + 2),
                        ThemeManager.Muted, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    // Thanh tiến trình ngang (Bar track & fill)
                    int trackX = x + labelW + 6;
                    int trackW = area.Right - rightW - trackX - 10;
                    if (trackW > 20)
                    {
                        var trackRect = new Rectangle(trackX, y + 6, trackW, 7);
                        using (var path = ThemeManager.RoundedRect(trackRect, 3))
                        using (var br = new SolidBrush(Color.FromArgb(242, 243, 248)))
                            g.FillPath(br, path);

                        // Độ dài tương ứng tỷ lệ %
                        double pct = Math.Max(0, Math.Min(100, it.TyLe));
                        int fillW = (int)Math.Round(trackW * (pct / 100.0));
                        if (it.SoLuong > 0 && fillW < 6) fillW = 6;

                        if (fillW > 0)
                        {
                            var fillRect = new Rectangle(trackX, y + 6, fillW, 7);
                            using (var path = ThemeManager.RoundedRect(fillRect, 3))
                            using (var br = new SolidBrush(col))
                                g.FillPath(br, path);
                        }
                    }

                    // Dòng mô tả nhỏ bên dưới thanh
                    TextRenderer.DrawText(g, it.MoTa ?? "", fMoTa, new Rectangle(trackX, y + 16, trackW, 14),
                        ThemeManager.Muted, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                }
            }

            // 3. Phân bố giới tính ở chân card
            int footerY = card.Bottom - 38;
            using (var pen = new Pen(ThemeManager.Line, 1))
                g.DrawLine(pen, x, footerY, x + w, footerY);

            if (_gioiTinh.Count > 0)
            {
                using (var fG = new Font("Segoe UI", 8F))
                using (var fGBold = new Font("Segoe UI", 8F, FontStyle.Bold))
                {
                    int curX = x;
                    TextRenderer.DrawText(g, "Giới tính: ", fGBold, new Point(curX, footerY + 8), ThemeManager.Ink2, TextFormatFlags.NoPadding);
                    curX += TextRenderer.MeasureText(g, "Giới tính: ", fGBold, Size.Empty, TextFormatFlags.NoPadding).Width + 4;

                    for (int j = 0; j < _gioiTinh.Count; j++)
                    {
                        var gt = _gioiTinh[j];
                        string icon = gt.TenGioiTinh.Contains("Nam") ? "Nam" : (gt.TenGioiTinh.Contains("Nữ") ? "Nữ" : gt.TenGioiTinh);
                        string str = $"{icon} {gt.TyLe:0.0}%";
                        if (j < _gioiTinh.Count - 1) str += "  ·  ";

                        TextRenderer.DrawText(g, str, fG, new Point(curX, footerY + 8), ThemeManager.Ink, TextFormatFlags.NoPadding);
                        curX += TextRenderer.MeasureText(g, str, fG, Size.Empty, TextFormatFlags.NoPadding).Width;
                        if (curX > area.Right) break;
                    }
                }
            }
        }
    }
}
